using System.Collections.Generic;
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
                //DrawLine(nearestNeighbor.Value.position, newNode.Value.position, Color.red, actor.transform, -1);
                if (Vector3.Distance(newNode.Value.position, endState.position) < 0.5)
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
        if (Random.value < 0.05)
        {
            return endState;
        }
        else
        {
            Vector3 pos = new Vector3(Random.Range(-boardWidth / 2.0f, boardWidth / 2.0f), endState.position.y, Random.Range(-boardHeight / 2.0f, boardHeight / 2.0f));
            Vector3 rot = new Vector3(0.0f, Random.Range(-180.0f, 180.0f), 0.0f);
            return new State(pos, rot, rot);
        }
    }

    private TreeNode<State> GetNearestNeighbor(State randState, TreeNode<State> tree)
    {
        TreeNode<State> nearestNeighbor = tree;
        float minDist = Vector3.Distance(randState.position, nearestNeighbor.Value.position);
        _getNearestNeighbor(randState, ref minDist, ref nearestNeighbor, tree);
        return nearestNeighbor;
    }

    private void _getNearestNeighbor(State randState, ref float minDist, ref TreeNode<State> nearestNeighbor, TreeNode<State> node)
    {
        foreach (var child in node.Children)
        {
            float newDist = Vector3.Distance(randState.position, child.Value.position);
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
        const float epsilon = 7.0f;
        const float minTurningRadius = 0.2f;
        const int numPoints = 10;

        actor.transform.position = start.position;
        float velDirection = Mathf.Rad2Deg * Mathf.Atan2(start.velocity.x, start.velocity.z);
        Vector3 forward = start.velocity.normalized;
        Vector3 diff = goalState.position - start.position;
        Vector3 x = diff - Vector3.Project(diff, forward);
        float radius = Vector3.SqrMagnitude(diff) / (2.0f * Vector3.Magnitude(x));
        if (radius < minTurningRadius)
        {
            newState = State.Undefined;
            return false;
        }
        x = Vector3.Normalize(x);

        float totalAngle = Vector3.Angle(-x, goalState.position - (start.position + radius * x));
        if (Vector3.Dot(forward, diff) < 0)
        {
            totalAngle = 360.0f - totalAngle;
        }
        float angleIncrement = (360.0f * epsilon / (2 * Mathf.PI * radius)) / (numPoints - 1.0f);
        if (Mathf.Abs(totalAngle / (numPoints - 1.0f)) < Mathf.Abs(angleIncrement))
            angleIncrement = totalAngle / (numPoints - 1.0f);
        Color randColor = new Color(Random.value, Random.value, Random.value);
        float curAngleOffset = 0;
        Vector3 point1 = start.position;
        Vector3 point2 = Vector3.positiveInfinity;
        for (int i = 0; i < numPoints; ++i)
        {
            //float pointingAngle = Vector3.SignedAngle(-x, point1 - (start.position + radius * x), Vector3.up) + start.rotation.eulerAngles.y;
            float pointingAngle = velDirection + curAngleOffset;
            newState = new State(point1, Quaternion.Euler(0.0f, pointingAngle, 0.0f), new Vector3(Mathf.Sin(Mathf.Deg2Rad * pointingAngle), 0.0f, Mathf.Cos(Mathf.Deg2Rad * pointingAngle)));
            if (actor.HitsObstacle(newState))
            {
                return i > 0;
            }

            curAngleOffset += angleIncrement;
            float radCurAngle = Mathf.Deg2Rad * curAngleOffset;
            point2 = start.position + radius * new Vector3(x.x * (1.0f - Mathf.Cos(radCurAngle)) - x.z * Mathf.Sin(radCurAngle), 0.0f, x.z * (1.0f - Mathf.Cos(radCurAngle)) + x.x * Mathf.Sin(radCurAngle));
            DrawLine(point1, point2, randColor, actor.transform, -1);

            point1 = point2;
        }
        float endAngle = velDirection + curAngleOffset;
        newState = new State(point1, Quaternion.Euler(0.0f, endAngle, 0.0f), new Vector3(Mathf.Sin(Mathf.Deg2Rad * endAngle), 0.0f, Mathf.Cos(Mathf.Deg2Rad * endAngle)));
        return true;
    }


    private List<State> CreatePathFromTree(TreeNode<State> tree, TreeNode<State> endNode)
    {
        List<State> path = new List<State>();
        TreeNode<State> cur = endNode;
        while (cur != null)
        {
            path.Insert(0, cur.Value);
            cur = cur.Parent;
        }
        return path;
    }


    private void DrawLine(Vector3 start, Vector3 end, Color color, Transform parent, float duration = -1.0f)
    {
        start.y += 0.1f;
        end.y += 0.1f;
        GameObject myLine = new GameObject("Line");
        myLine.transform.parent = parent;
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Unlit/Color"));
        lr.material.color = color;
        lr.startWidth = lr.endWidth = 0.2f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        if (duration >= 0.0f)
        {
            GameObject.Destroy(myLine, duration);
        }
    }


}
