# Royal Spin

## Game Overview
Royal Spin is a slot machine game developed using Unity. Players spin three reels using the lever, place bets, and try to match symbols to win credits.

### Features
- Three animated slot reels
- Lever-controlled spinning
- Multiple bet options: 10, 50, and 100 credits
- Credit balance and win tracking
- Payout system for matching symbols
- Reset option
- Audio effects and background music
- WebGL build for browser play

## Instructions to Run WebGL Build

1. Download or clone this repository.
2. Open the project using **Unity 2022.3**.
3. Open `Assets/Scenes/RoyalSpin.unity`.
4. To create a WebGL build, go to **File → Build Settings**.
5. Select **WebGL** and ensure the RoyalSpin scene is checked.
6. Click **Build** and select `Build/WebGL` as the output folder.
7. To play the existing WebGL build, serve the `Build/WebGL` folder using a local web server. Do not open `index.html` directly using `file://`.

## How to Play

1. Start the game with the available credits.
2. Select a bet amount.
3. Pull or click the lever to spin the reels.
4. Match symbols to win credits according to the payout rules.
5. Use the reset option to restart the game when needed.

## Bonus Features
- Animated reel movement
- Sound effects and background music
- Interactive betting controls
- Browser-compatible WebGL version

## Thought Process / Approach

The project is organized into separate scripts to make the game easier to maintain:

- **SlotGameManager.cs** — manages spins, bets, credits, payouts, and game statistics.
- **ReelController.cs** — controls reel animation and symbol movement.
- **GameAudioManager.cs** — manages background music and sound effects.
- **SlotMachineSceneBuilder.cs** — creates the slot machine scene and user interface through the Unity Editor.

The project uses Unity's UI system for player interaction and WebGL to make the game playable in a browser.

## Technologies Used
- Unity
- C#
- Unity UI
- WebGL

## Project Structure

```text
Royal-Spin/
├── Assets/
├── Packages/
├── ProjectSettings/
├── Build/
│   └── WebGL/
└── README.md
```

- **Assets/** — Game scenes, scripts, artwork, and other Unity assets.
- **Packages/** — Unity packages and dependency configuration.
- **ProjectSettings/** — Unity project configuration.
- **Build/WebGL/** — Compiled browser version of the game.
- **README.md** — Project overview and instructions.

## Author
Royal Spin — Unity Slot Machine Project
