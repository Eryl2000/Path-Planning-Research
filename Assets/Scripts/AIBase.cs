using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class AIBase
{
    protected Actor actor;

    protected AIBase(Actor _actor)
    {
        actor = _actor;
    }

    /* 
     * Param curState: The state the Actor is in currently.
     * Param targetState: The state the actor whishes to reach.
     * Param currentWaypoints: The current list of waypoints the actor has.
     * 
     * This function gives the AI an opportunity to change the actor's waypoints.
     * This is not necessary for all AI algorithms, but for ones such as RRT this
     * could be a place where it checks if it has found the goal, and if so
     * sets the waypoints accordingly.
     * 
     * If nothing needs to be done here, simply return currentWaypoints.
     */
    public abstract List<State> UpdateWaypoints(State curState, State targetState, List<State> currentWaypoints);

    /*
     * Param curState: The state the Actor is in currently.
     * Param targetState: The state the actor whishes to reach.
     * Param dt: The amount of time (seconds) to simulate forward.
     * 
     * This function is where the AI defines how the Actor can move from one state
     * to another. This could be as simple as moving directly toward the target,
     * it could involve potential fields, or it could be a more complex system.
     * 
     * Note that this function does NOT assume the Actor can reach targetState
     * this call. This could be because dt is too small, or because the Actor simply
     * cannot make it to targetState. Either way, this function should return the
     * state it can find which is closest to reaching the target.
     */
    public abstract State StepTowards(State curState, State targetState, float dt);
}
