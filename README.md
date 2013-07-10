PhoinxBot
=========

To use this bot, simply insert your bot's username and password in the app.config

If you want to add channels the bot should join, you can insert them into the database by adding this after Line 37 in the Form1.cs:
query += "INSERT INTO channels (name) ('YOURCHANNEL');";

And replace YOURCHANNEL with the channel you want.
