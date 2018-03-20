- Figure out how we're going to keep track of original draw position after setup because once a node falls, it loses it's original position.
- Figure out how we're going to add in new nodes, I'm thinking we spawn a node in the correct Y position and just let them fall then reassign all the nodes to the appropriate grid spot once we delete them

- we need to keep track of when nodes aren't sleeping before we allow any other action