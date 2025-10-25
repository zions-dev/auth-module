using System;
using System.Threading.Tasks;
using GameServer.Database.Entities;
using GameServer.Database.Repositories;
using GameServer.Extensions;
using GameServer.Infrastructure;
using GameServer.Modules.Notify;
using GameServer.Utils;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace GameServer.Modules.Auth
{
    public class AuthModule : Script, IGameModule
    {
        private static PlayerRepository _playerRepository;

        public Task InitializeAsync()
        {
            _playerRepository = Services.GetRequiredService<PlayerRepository>();

            Logger.LogInfo("[AuthModule] initialized");

            return Task.CompletedTask;
        }

        [RemoteEvent("Auth:Register")]
        public async Task Register(Player player, string email, string login, string password)
        {
            var (executed, (socialClubName, socialClubId)) =
                await MainThread.Run(() => (player.SocialClubName, player.SocialClubId), player);
            if (!executed)
                return;
            
            var normalizedEmail = email.NormalizeInput().ToLowerInvariant();
            var normalizedLogin = login.NormalizeInput();
            var normalizedPassword = password.NormalizePassword();
            
            var kEmail = $"register:email:{normalizedEmail}";
            var kLogin = $"register:login:{normalizedLogin}";

            if (string.CompareOrdinal(kEmail, kLogin) > 0)
                (kEmail, kLogin) = (kLogin, kEmail);

            await using var l1 = await _playerRepository.EnterSaveAsync(kEmail);
            await using var l2 = await _playerRepository.EnterSaveAsync(kLogin);

            var playerData = await _playerRepository.FindBySocialClub(socialClubId);
            if (playerData is not null)
            {
                player.SendNotify("Аккаунт уже привязан к вашему Social Club.", NotifyType.Error);
                return;
            }

            if (await _playerRepository.FindByEmailAsync(normalizedEmail) is not null)
            {
                player.SendNotify("Email уже используется. Войдите или восстановите доступ.", NotifyType.Error);
                return;
            }

            if (await _playerRepository.FindByLoginAsync(normalizedLogin) is not null)
            {
                player.SendNotify("Логин занят. Выберите другой.", NotifyType.Error);
                return;
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(normalizedPassword, workFactor: 12);

            try
            {
                var created = await _playerRepository.CreateAsync(new PlayerData
                {
                    SocialClubName = socialClubName,
                    SocialClubId = socialClubId,
                    Login = normalizedLogin,
                    Email = normalizedEmail,
                    PasswordHash = passwordHash,
                    CreatedAt = DateTime.UtcNow
                });

                player.SafeTriggerEvent("Auth:Register:Success");
                player.SendNotify($"Добро пожаловать, {created.Login}!", NotifyType.Success);
            }
            catch (DbUpdateException ex)
            {
                IsUniqueViolation(ex, out var field);
                switch (field)
                {
                    case "SocialClubId":
                        player.SendNotify("Ваш Social Club уже привязан к аккаунту. Попробуйте войти.",
                            NotifyType.Error);
                        break;
                    case "Email":
                        player.SendNotify("Email уже используется. Войдите или восстановите доступ.", NotifyType.Error);
                        break;
                    case "Login":
                        player.SendNotify("Логин занят. Выберите другой.", NotifyType.Error);
                        break;
                    default:
                        player.SendNotify("Не удалось создать аккаунт. Повторите позже.", NotifyType.Error);
                        break;
                }
            }
        }
        
        private static string ParseFieldFromConstraint(string constraintName)
        {
            if (string.IsNullOrEmpty(constraintName))
                return "unknown";

            constraintName = constraintName.ToLowerInvariant();

            if (constraintName.Contains("email"))
                return "Email";
            
            if (constraintName.Contains("login"))
                return "Login";
            
            return constraintName.Contains("socialclubid") ? "SocialClubId" : "unknown";
        }

        private static bool IsUniqueViolation(DbUpdateException ex, out string field)
        {
            field = null;
            
            if (ex.InnerException is not PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg) return false;
            
            field = ParseFieldFromConstraint(pg.ConstraintName);
            
            return true;

        }

        [RemoteEvent("Auth:Login")]
        public async Task Login(Player player, string login, string password)
        {
            var (executed, socialClubId) =
                await MainThread.Run(() => player.SocialClubId, player);
            if (!executed)
                return;
            
            var normalizedLogin = login.NormalizeInput();
            var normalizedPassword = password.NormalizePassword();

            var account = await _playerRepository.FindBySocialClub(socialClubId);
            if (account == null)
            {
                player.SendNotify("Аккаунт не найден.", NotifyType.Error);
                return;
            }

            if (!BCrypt.Net.BCrypt.Verify(normalizedPassword, account.PasswordHash))
            {
                player.SendNotify("Неверный пароль.", NotifyType.Error);
                return;
            }

            if (account.Login != normalizedLogin)
            {
                player.SendNotify("Неверный логин.", NotifyType.Error);
                return;
            }

            player.SetAccountId(account.Id);

            player.SafeTriggerEvent("Auth:Login:Success");

            executed = await MainThread.Run(() => NAPI.Entity.SetEntityTransparency(player, 255), player);
            if (!executed)
                return;

            player.SendNotify($"Приветствуем снова, {account.Login}!", NotifyType.Success);
        }

        [RemoteEvent("Auth:SendResetCode")]
        public async Task SendResetCode(Player player, string email)
        {
            var target = await _playerRepository.FindByEmailAsync(email.NormalizeInput());
            if (target == null)
            {
                player.SendNotify("Аккаунт с таким email не найден.", NotifyType.Error);
                return;
            }

            var code = new Random().Next(100000, 999999).ToString();
            target.ResetCode = code;
            await _playerRepository.SaveAsync(target);

            player.SafeTriggerEvent("Auth:SendResetCode:Success");

            player.SendNotify($"Ваш код: {code}", NotifyType.Success);
        }

        [RemoteEvent("Auth:ResetPassword")]
        public async Task ResetPassword(Player player, string email, string code, string newPassword)
        {
            var result = await _playerRepository.ResetPasswordAsync(email.NormalizeInput(), code, newPassword);
            if (!result)
            {
                player.SendNotify("Код сброса недействителен или истёк.", NotifyType.Error);
                return;
            }

            player.SafeTriggerEvent("Auth:ResetPassword:Success");

            player.SendNotify("Пароль успешно обновлён", NotifyType.Success);
        }
    }
}