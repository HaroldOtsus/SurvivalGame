# SurvivalGame / Ellujäämise mäng

SurvivalGame is a 2D top-down shooter survival computer game developed for Windows OS.

The game was made with the Unity game engine and scripts were written with Visual Studio.

Key features:
- Singleplayer.
- 3 types of enemy behavior algorithms.
- Mobile and infinitely spawning enemies.
- 3 types of enemies. Pistol, melee, shield enemies.
- Player has 2 types of offensive weapons(Pistol and Baseball bat) and 1 defensive shield.

Enemy algorithms:
- Easy algorithm - Enemies spawn in static waves and attack the player.
- State algorithm - Enemies spawn all the time and randomly and patrol the area. When detected enemies will attack the player. Enemies also escape from the player once their health reaches a certain threshold.
- Adaptive algorithm - Enemies spawn in dynamic waves based on different types of enemies destroyed the most(Example: if pistol enemies are destroyed the most, they will spawn less frequently). Pistol enemies strafe when shooting the player and also back away from the player if player gets too close. Shield enemies will actively protect pistol enemies by standing in front of them with their shield up.

Installation:
- Download the project.
- The game is located in the 'Build' folder.
- Run the 'SurvivalGame.exe' file .

Gameplay tutorial:
- The character's movement is controllable with the 'WASD' keys.
- Weapons can be changed with the 'TAB' key.
- Shooting the pistol and swinging the bat is with the 'Mouse 1' key.
- Aiming and looking around is with the mouse.

Used assets:
- Cainos, https://cainos.itch.io/pixel-art-top-down-basic .
- ChrisJulch, https://chrisjulch.itch.io/top-down-shooter-pixel-art .
- L. Taluste, some assets created by author's request.

Demos:
- State algorithm - https://youtu.be/ZmYOgdNOaQY .
- Adaptive algorithm - https://youtu.be/ciM1bvvQZbQ .

----


SurvivalGame on Windows OS-i jaoks arendatud 2D ülaltvaates tulistamise ellujäämismäng.

Mäng loodi Unity mängumootoriga ning skriptid kirjutati Visual Studio abil.

Põhilised funktsioonid:
- Üksik mängija jaoks mõeldud.
- 3 tüüpi vastaste algoritme.
- Liikuvad ja lõputult tekkivad vastased.
- 3 tüüpi vastaseid. Püstoliga, kurikaga, kilbiga vastased.
- Mängijal on kahte tüüpi relvi(Püstol ja kurikas) ning kaitseks kilp.

Vastaste algoritmid:
- Lihtne algoritm - Vastased ilmuvad staatiliste lainete viisil ja ründavad mängijat.
- Oleku algoritm - Vastased ilmuvad kogu aeg juhuslikult ja patrullivad mänguala. Kui vastased tuvastavad mängija, siis ründavad mängijat. Vastased põgenevad, kui nende elu punktid lähevad liiga madalale.
- Adaptiivene algoritm - Vastased ilmuvad dünaamiliste lainete viisil sõltuvalt, mis vastase tüüpi on kõige rohkem hävitatud(Näide: Kui püstoliga vastaseid on kõige rohkem hävitatud, siis neid ilmub vähem). Püstoliga vastased liiguvad külg suunaliselt kui tulistavad ja liiguvad mängijast eemale, kui mängija liiga lähedale neile läheb. Kilbiga vastased aktiivselt kaitsevad püstoliga vastaseid liikudes nende ette kilbiga.

Installeerimine:
- Lae alla projekt.
- Mäng asub 'Build' kaustas.
- Ava mängu rakendus 'SurvivalGame.exe' failist.

Mängimise juhend:
- Mängitavat karakterit saab liigutada kasutades 'WASD' nuppe.
- Relvi saab vahetada kasutades 'TAB' nuppu.
- Püstolit ja kurikat saab kasutada 'Mouse 1', ehk vasak poolse hiire nupuga.
- Ringi vaadata ja sihtida saab hiirega.

Kasutatud materjalid:
- Cainos, https://cainos.itch.io/pixel-art-top-down-basic .
- ChrisJulch, https://chrisjulch.itch.io/top-down-shooter-pixel-art .
- L. Taluste, mõned mänguvarad tehtud autori soovil.

Demod:
- Oleku algoritm - https://youtu.be/ZmYOgdNOaQY .
- Adaptiivne algoritm - https://youtu.be/ciM1bvvQZbQ .