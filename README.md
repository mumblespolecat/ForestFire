<p><strong>GameOfLife Broken Vesion</strong></p>

<p>This is released to the students of VR MSC as broken code example. Any one using this not in the university will be disappionted as its incomplete.</p>
<hr>
<p>Since the initial setting up of the reposiory, I have added features to the 2D and 3D Forest Fire simulations as follows:</p>
<ul>
<li>Wind to both 2D and 3D, with both speed and direction, which has a noticable effect on the speed and direction of the spread of the fire. It is all done with random numbers and a 1 - 8 direction value (1 is north, then moving clockwise round to 8 for NW) and a 0 - 2 speed value.  It then increases the count for number of alight neighbours depending on the direction and speed. The idea was posed by Alex, the initial script guided by Steve and the final implementatuion by me.</li>
<li>Random river / water feature generation, which as it has no fuel, acts as a fire break. Works well in 2D. Same river / water feature gereration added to 3D, but although it looks good on both the 3D ground and the minimap, prior to starting the simulation, as soon as it is un-paused, it breaks and all cells that were water are now set to burnt (on both map and ground).  Something in the code (as yet unknown) turns all the water to burnt ground.  The fire break properties remain, but it isn't as intended. (the cause of this has now (12th November) been identifed by Ted).</li>
</ul>
