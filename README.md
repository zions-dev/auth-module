# Auth System

This folder contains only the authorization system extracted from the mod.

Included parts:

- server-side: Auth module, Player entity, Player repository, string normalizers, and connect flow (GlobalEvents).
- client-side: RAGE Multiplayer auth events and CEF bridge helper.
- cef: Vue Auth view and Pinia store.

Notes:

- These files depend on the original project (EF Core context, DI, caching, GTANetworkAPI, UI modules). They compile and work inside the full mod. In isolation they are for portfolio demonstration.
- To see the UI in the running mod: connect → GlobalEvents triggers `Auth:ShowLogin` which opens the Auth route in UI.

Minimal integration hints (if you want to run auth standalone):

- Server: implement a minimal `ApplicationDbContext` with `DbSet<PlayerData>` and wire `PlayerRepository` with stubbed `IRedisCache`/`IDbConcurrencyGate`, or replace cache calls with no-ops.
- Client: register `registerAuthEvents()` and ensure `UI.enableInterface('Auth')` routes to the Auth page.
- CEF: add the Auth route in your Vue router and mount `useAuthStore`.
