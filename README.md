# Connect-Four

This is a 3D clone of the two-player connection board game, Connect Four, created in the Unity Engine scripted in C#. The key features this game has to offer is online multiplayer and an AI that gets progressively harder each time you beat it.

The players choose a color and then take turns dropping colored discs into a seven-column, six-row vertically suspended grid. The pieces fall straight down, occupying the lowest available space within the column. The objective of the game is to be the first to form a horizontal, vertical, or diagonal line of four of one's own discs. The first player can always win by playing the right moves.

For optimization purposes, this game was designed under an event-driven architecture where no code is run in an Update() function. So instead, only coroutines and C# delegates are used for example for input detection and moving the objects around.

## Installation
1. Download & extract the ZIP file
3. Open the extracted folder 
4. Go to the Builds folder 
* Windows - Click Connect4.exe
* Android - Put Connect4.apk to Android phone

## How to Play
1. Open the application on whichever operating system you are running on
2. Select **Local Game** button to play against the AI
3. Select **Quick Game** button to play against a random player on a different device (**MUST ENTER IN USERNAME**)
4. Select the black disc or white disc
5. Press Play Button

* Press Reset Button to play another round of Connect-4 after game has finished
* Press Back Button to go back to start menu

### Controls
|         Instruction           | Touch Controls |
| ----------------------------- | -------------- |
| Move Disc to the Left         | Swipe Left     |
| Move Disc to the Right        | Swipe Right    |
| Drop Disc                     | Swipe Down     |

* On Windows, swipe controls are activated by holding the left mouse button

## Credits
I created all of the 3D models in Blender (Black Disc, White Disc, & Game Board), including the pictures for the black & white disc. The other 2D assets were from the asset store.
* The Connect Four title asset originated from [PSD Logo Templates by Unruly Games](https://assetstore.unity.com/packages/2d/gui/icons/psd-logo-templates-103928)
* The rest of the user interface assets were from [Simple UI by Unruly Games](https://assetstore.unity.com/packages/2d/gui/icons/simple-ui-103969)
