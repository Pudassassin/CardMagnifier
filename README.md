# Card Magnifier [0.1.0] 
Public Release 0-1-0, build #27

Zoom, Enlarge and bring closer to center the highlighted card during card pick and card bar preview for better readability! A handy client-side tool made by Pudassassin.

## **Presets**

#### \[Vanilla+]
- default at first installation
- zoom the card to fixed, more readable size while slightly pull the card toward the center of the screen

#### \[Fancy Zoom]*
- zoom it BEEG, and always pull the card toward center with transition time*
- \*may cause motion sickness in rare case, fair warning!

#### \[Insta-Zoom!]
- immediately enlarge the card and put it right in the center, no flipping!

#### \[Reduce Card Motions]
- remove transition time, card flipping when first revealed, card bobbing when (re)highlighted and reduce discarded cards motion
- clicking this keep other settings intact

## **Card Zoom Setting**

#### \[Card-to-ZoomPoint Position]
- the mid-way point value between the card's original position and the specified zoom point
- 0.0 leaves it at where it was, 1.0 fixes it to the zoom point

#### \[ZoomPoint Position]
- the (x,y) coord. of where ther card would be zoomed and moved toward to
- (0.0, 0.0) being perfect center of the screen

#### \[Card Zoom Scale]
- the overall scale the highlighted card will be enlarged by
- scales relatively from the size it was spawned at

#### \[Zoom Card to Fixed Size]
- make [Card Zoom Scale] option an absolute value regardless of how big/small the card was spawn at

#### \[Reorient Zoomed Card Upright]
- says itself-- highlighted card rotates the right way up!

#### \[Card Zoom Animation Time]
- time it takes to move the card from original position to zoom point

#### \[Decrease Discard Card Motions]
- originally the discard effect was not as dramatic without this mod, this option turn off the extras and use the vanilla's animation

#### \[Disable Card Bobbing when Highlighted]
- disable the card bobbing effect when swiping around between card choices, making instant-zoom even more static and instant!

#### \[Disable Card Flipping when Revealed]
- disable the flipping animation where the back-facing card is first highlighted

## **CardBar Preview Setting**

#### \[Preview Position]
- overrides the position where card is spawned while hovering the Card Bar
- (0.0, 0.0) being perfect center of the screen-- you definitely don't want that in the heat of the battle!

#### \[CardBar Preview Size]
- the size of the preview card to be spawned at

## **Note from the modder**
Hopefully this mod will resolve a lot of issues about card offers that go off-screen or too small to read. Automatically adapt to screen resolutions and MapEmbiggener's scaling

## Patch Notes
#### Public Beta 1-0 \[v0.1.0]
- It all begins. Core functionality with in-game options and interactive demo!
