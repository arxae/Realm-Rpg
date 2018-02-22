# Realm

Play a RPG inside your discord chanel.

## Installation
1. Create a RavenDB instance somewhere
2. Create a *rpg* and *rpg_logs* database
3. Import the Rpg_Default_data.ravendbdump file into a database
4. Modify the Urls and Database property in Db.cs to point to the correct instance url and database (Put in config is todo)
5. Set a *REALMBOT_KEY* user environmental variable containing your bot key.
6. In discord, create a "Realm Admin" role (and assign it to yourself i needed)
7. Start bot

Should you use a secured service. Generate a certificate with at least read/write rights to the *rpg* and *rpg_logs* databases. Drop this certificate into the output directory.
The first *.pfx* certificate will be loaded.

Should you use a password for your certificate, put it in a user environmental variable called *REALMBOT_CERT_KEY*