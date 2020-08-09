# Toha-Heavy-Industries
A small Discord Bot that mainly broadcasts active Twitch streams for some categories 

Overview:
 Function:
 General: Bot Initialisation -> set paths -> load monitor instances from file -> create monitor instances -> keep listening for commands
 Monitor: create Timer -> on Timer event get Twitch data for a streamcategory -> parse data -> broadcast valid streams to discord

 

Classes:
 Program.cs: core of the program, sets up the discord bot, global variables, a twitch api client
             loads monitor instances and adds them to a StreamMonitorHandler
             
 Misc.cs: contains frequently used Methods for sending Discord Messages  
 
 Twitch.
   StreamsMonitor.cs: 
      1. Setup a timer that calls every x milliseconds a function                
      2. this function calls the twitch api data for a set category
      3. the function parses the data, checks for whitelist,bans,spam
         and maybe broadcasts it to a set discord-server, discord-channel
                         
   StreamsMonitorHandler.cs: 
      contains the StreamsMonitor instances, banlist, whitelist
      and methods to load,save the instances from,to txt files
                             
   MonitorData.txt: contains the monitor setup information by line in packs of 5: 
      1. discord-server 
      2. discord-channel    
      3. twitch-category
      4. timer-intervall in ms
      5. limit of concurrent streams          
     Note: this file doesnt get used if there is a different Path specified in Data.Paths.monitor_instances_txt
                             
  Imgur.
    Download.cs: downloads stream previews in order to be able to upload them to Imgur
    Imgur.cs: uploads stream previews to Imgur in order to prevent grey pictures in the discord channels
    
  Data.
    APIToken.cs: contains the api keys for Discord, Twitch, Imgur
    Gate.cs: helps loading,saving the whitelist, banlist(Streamers)
    Paths.cs: searches for a THI-paths.txt file that overrides the default paths 
              for the monitor instances.txt, the image preview archive and the meme directory
    Streamers.txt: contains the banned twitch channels, applies to all monitor instances
    Whitelist.txt: whitelist for Descent related channels
    
    
    
    
    
  To-do:
   Fix this readme file since md apparently doesnt use spaces the way god intended
   Imgur.Download.cs nearly obsolete, discord supports just uploading the file directly, lets skip the imgur part once the keys run out
   Imgur.Imgur.cs -||-
   Data.APIToken.cs populate this from a file to make this bot reusable without the need for recompiling
                             
                             
           
           
