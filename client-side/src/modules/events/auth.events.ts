function registerAuthEvents() {
    mp.events.add('Auth:ShowLogin', () => {
        UI.show('Auth');
        mp.gui.cursor.visible = true;
    })

    mp.events.add('Auth:Register:Success', () => {
        UI.invokeStoreAction('auth', 'goToLogin');
    })

    mp.events.add('Auth:Login:Success', () => {
        UI.hide();
    })

    mp.events.add('Auth:SendResetCode:Success', () => {
        UI.invokeStoreAction('auth', 'goToResetPasswordStep');
    })

    mp.events.add('Auth:ResetPassword:Success', () => {
        UI.invokeStoreAction('auth', 'goToLogin');
    })

    mp.events.add('Auth.Register', (email: string, login: string, password: string) => {
        mp.events.callRemote("Auth:Register", email, login, password);
    })

    mp.events.add('Auth.Login', (login: string, password: string) => {
        mp.events.callRemote("Auth:Login", login, password);
    })

    mp.events.add('Auth.SendResetCode', (email: string) => {
        mp.events.callRemote("Auth:SendResetCode", email);
    })

    mp.events.add('Auth.ResetPassword', (email: string, code: string, newPassword: string) => {
        mp.events.callRemote("Auth:ResetPassword", email, code, newPassword);
    })
}

registerAuthEvents();