# FunManipulator

A command-line tool designed for manipulating the RNG used for the "Fun" value in Undertale.
Default configuration is currently designed for Undertale 1.001 Linux running under Windows GameMaker runner.

## How to use
This section will likely be expanded upon later, but you first supply the fun value range you desire, as the program prompts you.
It will then wait for you to press spacebar. This is intended for use with Alt+Space buffering, a method used to pause the game, although it is not strictly required.

The purpose of pausing is to locate the random shake positions of the first three rows of letters on the naming screen.
Ideally, you would use an overlay image, such as through a program like OBS, to do this for you. One such image is provided in this repository (`naming overlay.png`).

As for the specific technique, this is what the default configuration (editable inside of `config.json`) is currently designed for:
- Press Z+Alt+Space together.
- Repeatedly press Alt+Space until the screen changes to the Yes/No selection screen, while in a program like OBS, the preview shows the previous frame.
- Use keys 1 through 4 to input positions, as prompted. Do not unfocus the window (the game needs to stay paused).
- Wait for the program to perform some calculations, and then it should prompt you to press the Alt key to continue.
- After pressing Alt, a countdown timer begins. Be sure not to accidentally pause the game (such as by dragging it), and hover over "Yes" on the screen.
- The countdown will end with five (again, this is configurable) beeps, the last of which should be exactly when you press Z/Enter to select the name.
- The program will try and tell whether your input was early or late, though it can have false positives and possibly negatives.
- If all goes well, your fun value should be in your desired range! If not, it will likely be one of the "surrounding" values, which are printed to the console.

## Special Thanks
Huge thanks to everyone in the Undertale speedrunning community involved with the techniques and discussion around this.
In particular, thanks to OceanBagel and chair for their input.