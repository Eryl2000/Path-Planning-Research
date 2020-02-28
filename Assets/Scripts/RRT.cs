using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI_RRT : AI_PotentialFieldFollowWaypoints
{
    public int maxNumNodes;

    bool successful;
    bool finished;
    TreeNode<State> treeBase;
    State targetState;
    TreeNode<State> finalNodeAtTarget;
    int numNodesAdded;

    public AI_RRT(Actor _actor) : base(_actor)
    {
        finished = successful = false;
        maxNumNodes = 50000;
        numNodesAdded = 0;
        treeBase = null;
        targetState = null;
        finalNodeAtTarget = null;
    }

    public override List<State> UpdateWaypoints(State curState, State targetState, List<State> currentWaypoints)
    {
        if (treeBase == null)
        {
            treeBase = new TreeNode<State>(curState);
            this.targetState = targetState;
        }
        for (int i = 0; i < 10; ++i)
        {
            NextStep();
        }
        if (finished)
        {
            finished = false;
            List<State> ret;
            if (successful)
            {
                successful = false;
                ret = CreatePathFromTree(finalNodeAtTarget);
            }
            else
            {
                ret = currentWaypoints;
            }
            treeBase = null;
            this.targetState = null;
            finalNodeAtTarget = null;

            return ret;
        }
        else
        {
            return currentWaypoints;
        }
    }

    public void NextStep()
    {
        if (finished)
        {
            return;
        }
        const int maxTriesToAddNode = 10000;
        int triesToAddNode = 0;
        bool addedNode;
        do
        {
            addedNode = false;
            State randState = GetRandomState();
            TreeNode<State> nearestNeighbor = GetNearestNeighbor(randState);

            //Select input to use
            //For now any input is valid

            //Determine new state
            State newState = State.Undefined;
            if (TryStepTowards(nearestNeighbor.Value, randState, actor, ref newState))
            {
                addedNode = true;
                TreeNode<State> newNode = nearestNeighbor.AddChild(newState);
                if (actor.ReachedWaypoint(newNode.Value, targetState))
                {
                    finalNodeAtTarget = newNode;
                    successful = true;
                    finished = true;
                    return;
                }
            }
            triesToAddNode += 1;
            if (triesToAddNode > maxTriesToAddNode)
            {
                successful = false;
                finished = true;
                return;
            }
        } while (!addedNode);

        numNodesAdded++;
        if (numNodesAdded >= maxNumNodes)
        {
            successful = false;
            finished = true;
        }
    }


    private State GetRandomState()
    {
        if (Random.value < 0.04)
        {
            return targetState;
        }
        else
        {
            Vector3 pos = new Vector3(Random.Range(-SceneManager.Instance.BoardX / 2.0f, SceneManager.Instance.BoardX / 2.0f), targetState.position.y, Random.Range(-SceneManager.Instance.BoardZ / 2.0f, SceneManager.Instance.BoardZ / 2.0f));
            Vector3 rot = new Vector3(0.0f, Random.Range(-180.0f, 180.0f), 0.0f);
            return new State(pos, rot, rot.y, actor.ShipData.CruiseSpeed);
        }
    }

    private TreeNode<State> GetNearestNeighbor(State randState)
    {
        TreeNode<State> nearestNeighbor = treeBase;
        float minDist = actor.ClosenessMeasure(randState, nearestNeighbor.Value);
        _getNearestNeighbor(randState, ref minDist, ref nearestNeighbor, treeBase);
        return nearestNeighbor;
    }

    private void _getNearestNeighbor(State randState, ref float minDist, ref TreeNode<State> nearestNeighbor, TreeNode<State> node)
    {
        foreach (var child in node.Children)
        {
            float newDist = actor.ClosenessMeasure(randState, child.Value);
            if (newDist < minDist)
            {
                minDist = newDist;
                nearestNeighbor = child;
            }
            _getNearestNeighbor(randState, ref minDist, ref nearestNeighbor, child);
        }
    }

    private bool TryStepTowards(State start, State goalState, Actor actor, ref State newState)
    {
        const float timeToSimulate = 60.0f;
        const int numPoints = 3;

        //Store points so we can draw the path after confirming it is valid
        List<Vector3> points = new List<Vector3>();
        points.Add(start.position);

        newState = start;
        for (int i = 0; i < numPoints; ++i)
        {
            newState = base.StepTowards(newState, goalState, timeToSimulate / numPoints);
            if (actor.WouldHitObject(newState))
            {
                newState = State.Undefined;
                return false;
            }
            if (actor.ReachedWaypoint(newState, targetState))
            {
                points.Add(newState.position);
                break;
            }

            //if (i % 7 == 0)
            //{
            //    points.Add(newState.position);
            //}
        }

        //Draw path
        //Color randColor = new Color(Random.value, Random.value, Random.value);
        //for (int i = 0; i < points.Count - 1; ++i)
        //{
        //    SceneManager.DrawLine(points.ElementAt(i), points.ElementAt(i + 1), randColor, actor.transform, -1);
        //}
        return true;
    }


    private List<State> CreatePathFromTree(TreeNode<State> endNode)
    {
        List<State> path = new List<State>();
        TreeNode<State> cur = endNode;
        while (cur != null)
        {
            path.Insert(0, cur.Value);

            //float pointSize = 20.0f;
            //DrawLine(cur.Value.position - pointSize / 2.0f * Vector3.forward, cur.Value.position + pointSize / 2.0f * Vector3.forward, new Color(0f / 255, 46f / 255, 98f / 255), actor.transform, -1, pointSize, 1);
            State target = cur.Value;
            cur = cur.Parent;

            if (cur != null)
            {
                State temp = cur.Value;
                for (int i = 0; i < 4; ++i)
                {
                    State temp2 = actor.AI.StepTowards(temp, target, 8.0f / 3.0f);
                    //SceneManager.DrawLine(temp.position, temp2.position, Color.red, actor.transform, -1, 15f, 1);
                    temp = temp2;
                }
            }
        }
        return path;
    }


}
