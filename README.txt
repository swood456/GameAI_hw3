1. What did you do for avoiding a group of agents? What are the weights of path following and evade behavior?
The code as written uses collision prediction to avoid colliding with any individual or group in the scene. To view the scene with cone check instead, there is a public Boolean “usingConeCheck” that can be toggled from the prefabs for both flocks. The weights of collision avoidance and flocking behavior are all the same, which has seemed to work well.


2. What are the differences in cone check and collision prediction’s performances?
Cone Check does not avoid fellow flock members very well, as it does not adjust for flock members to the side of the checker, or the velocities of flock members that are detected. Using this mode tends to cause flocks to become fragmented, though some adjustment of the weighting of collision avoidance and cohesion may fix this.