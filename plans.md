Spawning & Moving Missing Tiles as well as updating grid nodes with new instances
============================

So currently the way we have things setup is we loop through the grid and we find branches for individual nodes. Every time we find a branch in the chain we store all the blocks, run some recursion until we find the max amount of bricks in a chain. Once that's done we either highlight them or move to the next column in a row. 

When we found a branch we start to highlight all the nodes in the branch and queue them up for deletion. We start a tile destroy timer which after a certain amount of time passes it'll then proceed to delete those tiles then stop the timer and start the recursive array over. Once we get through every node and find every obvious match we then move on and attempt to start moving tiles, spawning new tiles and updating/moving nodes to the correct grid.

So how do we and in which order do we:
- update grid nodes to be associated with the correct grid piece
- move grid items down to their appropriate position
- spawn new nodes and move them to their appropriate positions

Lets try:
- need to loop through each row in reverse for each col to see if the current tile is floating in mid air
    - then loop until you find the position you're supposed to be and reassign the node to the correct grid
- check all tile nodes that aren't in the correct position vertically and start moving them every frame based on (velocity) until it gets to the correct position it's supposed to be at
    - need a check to tell the script that this is true
        - when we add this check we need to reset this check when we find all checks again or when we manually attempt to switch a tile
- spawn nodes for missing position and hide the nodes
- loop through nodes backwards and start mov


---------------------------


Dealing with Tile Grid Code
===========================

Grid Class:
- use this class to generate our default grid and values
- by default all grid items start at a value
- when a grid item is missing note that

Questions:
- How do we tie the grid code with nodes?
- With our grid class how should we generate the node positions?
- How do we spawn in new tiles to take advantage of the gravity physics?

Notes:
- When a row loses blocks it readds missing blocks for each missing/matched tile
- RigidBody2d has a `sleeping` variable that allows us to know when a moving object isn't moving, that way we can deal with turning each rigidbody into a kinematic body when all nodes are sleeping which allows us to move/match tiles without the gravity affecting anything.
    - We need all nodes to be sleeping before we can select nodes again
- selecting a node changes the tile's animation
    - we can only have two tiles selected at one time


01, 02, 03, 04, 05, 06, 
07, 08, 09, 10, 11, 12, 
13, 14, 15, 16, 17, 18,
19, 20, 21, 22, 23, 24,
25, 26, 27, 28, 29, 30, 
31, 32, 33, 34, 35, 36

for(x=0, x < 6, x++) {
    for(y=0, y<6, y++) {

    }
}

0:0, 0:1, 0:2, 0:3, 0:4, 0:5, 0:6
1:0, 1:1, 1:2, 1:3, 1:4, 1:5, 1:6
2:0, 2:1, 2:2, 2:3, 2:4, 2:5, 2:6
3:0, 3:1, 3:2, 3:3, 3:4, 3:5, 3:6
4:0, 4:1, 4:2, 4:3, 4:4, 4:5, 4:6
5:0, 5:1, 5:2, 5:3, 5:4, 5:5, 5:6

Tile Chain Removal
===================

Now that I have code to find matches and branches off of those matches, I need to figure out a way to queue up the removable of the tiles
- should i instantly remove them or highlight them as success tile bg wait a few seconds then process the next lot?