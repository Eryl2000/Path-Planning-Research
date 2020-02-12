using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RRT
{
    public bool Successful;
    public bool Finished;

    TreeNode<State> tree = null;
    State startState;
    State endState;
    Actor actor;
    int numNodesAdded;
    int maxNumNodes;
    float boardWidth;
    float boardHeight;

    TreeNode<State> m_endNode;

    public RRT(State _startState, State _endState, Actor _actor, int _maxNumNodes, float _boardWidth, float _boardHeight)
    {
        startState = _startState;
        endState = _endState;
        actor = _actor;
        numNodesAdded = 0;
        maxNumNodes = _maxNumNodes;
        boardWidth = _boardWidth;
        boardHeight = _boardHeight;

        Finished = Successful = false;
        tree = new TreeNode<State>(_startState);
    }

    public void NextStep()
    {
        int triesToAddNode = 0;
        bool addedNode;
        do
        {
            addedNode = false;
            State randState = GetRandomState();
            TreeNode<State> nearestNeighbor = GetNearestNeighbor(randState, tree);

            //Select input to use
            //For now any input is valid

            //Determine new state
            State newState = State.Undefined;
            if (StepTowards(nearestNeighbor.Value, randState, actor, ref newState))
            {
                addedNode = true;
                TreeNode<State> newNode = nearestNeighbor.AddChild(newState);
                if (actor.ReachedWaypoint(newNode.Value, endState))
                {
                    m_endNode = newNode;
                    Successful = true;
                    Finished = true;
                    return;
                }
            }
            triesToAddNode += 1;
            if (triesToAddNode > 10000)
            {
                Successful = false;
                Finished = false;
                return;
            }
        } while (!addedNode);

        numNodesAdded++;
        if (numNodesAdded >= maxNumNodes)
        {
            Successful = false;
            Finished = true;
        }
    }

    public List<State> GetPath()
    {
        if (Finished && Successful)
        {
            return CreatePathFromTree(tree, m_endNode);
        }
        else
        {
            return null;
        }
    }

    private State GetRandomState()
    {
        if (Random.value < 0.04)
        {
            return endState;
        }
        else
        {
            Vector3 pos = new Vector3(Random.Range(-boardWidth / 2.0f, boardWidth / 2.0f), endState.position.y, Random.Range(-boardHeight / 2.0f, boardHeight / 2.0f));
            Vector3 rot = new Vector3(0.0f, Random.Range(-180.0f, 180.0f), 0.0f);
            return new State(pos, rot, rot.y, actor.CruiseSpeed);
        }
    }

    private TreeNode<State> GetNearestNeighbor(State randState, TreeNode<State> tree)
    {
        TreeNode<State> nearestNeighbor = tree;
        //float minDist = Vector3.Distance(randState.position, nearestNeighbor.Value.position);
        float minDist = actor.ClosenessMeasure(randState, nearestNeighbor.Value);
        _getNearestNeighbor(randState, ref minDist, ref nearestNeighbor, tree);
        return nearestNeighbor;
    }

    private void _getNearestNeighbor(State randState, ref float minDist, ref TreeNode<State> nearestNeighbor, TreeNode<State> node)
    {
        foreach (var child in node.Children)
        {
            //float newDist = Vector3.Distance(randState.position, child.Value.position);
            float newDist = actor.ClosenessMeasure(randState, child.Value);
            if (newDist < minDist)
            {
                minDist = newDist;
                nearestNeighbor = child;
            }
            _getNearestNeighbor(randState, ref minDist, ref nearestNeighbor, child);
        }
    }

    private bool StepTowards(State start, State goalState, Actor actor, ref State newState)
    {
        const float timeToSimulate = 8.0f;
        const int numPoints = 22;

        //Store points so we can draw the path after confirming it is valid
        List<Vector3> points = new List<Vector3>();
        points.Add(start.position);

        newState = start;
        for (int i = 0; i < numPoints; ++i)
        {
            newState = actor.StepTowards(newState, goalState, timeToSimulate / numPoints);
            if (actor.WouldHitObject(newState))
            {
                newState = State.Undefined;
                return false;
            }
            if (actor.ReachedWaypoint(newState, endState))
            {
                points.Add(newState.position);
                break;
            }

            if (i % 7 == 0)
            {
                points.Add(newState.position);
            }
        }

        //Draw path
        Color randColor = new Color(Random.value, Random.value, Random.value);
        for (int i = 0; i < points.Count - 1; ++i)
        {
            SceneManager.DrawLine(points.ElementAt(i), points.ElementAt(i + 1), randColor, actor.transform, -1);
        }
        return true;
    }


    private List<State> CreatePathFromTree(TreeNode<State> tree, TreeNode<State> endNode)
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
                    State temp2 = actor.StepTowards(temp, target, 8.0f / 3.0f);
                    SceneManager.DrawLine(temp.position, temp2.position, Color.red, actor.transform, -1, 15f, 1);
                    temp = temp2;
                }
            }
        }
        return path;
    }


}
