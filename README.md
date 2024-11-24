# WARNING

## VAPSR is still extremely unstable. Expect crashes and features not functioning.

## The custom cursor is also relatively buggy, expect issues with it.

# vapsr-client

the client for va proxy speedrun 1v1s.

System.Reflection.Emit and System.Reflection.Emit.ILGeneration dlls need to be manually copied over from (the-nuget-package)/4.0.0/lib/netcore50 (for some reason, they're not added automatically)

if you want to use this, you or your friend will have to host an instance of [vapsr-server](https://github.com/tairasoul/vapsr-server) and setup config.

you will have to manually set your username in BepInEx/config/vainsoul.vaproxy.vapsrclient.cfg and the ip you connect to.

if you want to play with your friends without port forwarding, i'd recommend something like [hamachi](https://www.vpn.net/)


## DISCLAIMER

not sure how but some people have mistaken this for va proxy multiplayer

this is not va proxy multiplayer. this is more akin to MCSR1V1 (minecraft speedrun 1v1) than actual multiplayer.

## known issues

crashes happen sometimes when loading into a match, when opening code screen, when creating a room, and probably a few more i don't know about

placement screen in private rooms doesn't let you scroll (still don't know why)

custom cursor sometimes just doesn't work