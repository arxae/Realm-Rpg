# Realm - Discord RPG
Rpg played completely in Discord. 

## Quick Start Notes:
1. Setup RavenDB instance with a ```rpg``` and ```rpg_logs``` database
2. Import the ```Rpg_Default_Data.ravendbdump``` file into the ```rpg``` database
3. Modify ```Servers.txt``` to point to your RavenDb instance.
4. Set a environmental variable called ```REALMBOT_KEY``` containing your own Discord bot key.
5. Make sure a ```Realm Admin``` role exists on your server
6. Start bot

In case you use a secure RavenDb instance, generate a certificate with read/write rights to the ```rpg``` and ```rpg_logs``` database. Drop the .pfx file into the bot directory. The first .pfx file found will be used.

If your certificate requires a passkey, enter your password into a ```REALMBOT_CERT_KEY``` environmental variable.

To build the documentation, clone the repo and run the ```BuildDocs.ps1``` command. This will build the documentation and and serve it on localhost port 8081