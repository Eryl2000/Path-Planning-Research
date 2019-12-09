using System.Collections.Generic;
using UnityEngine;

public class RRT
{
    public bool Successful;
    public bool Finished;

    TreeNode<Vector3> tree = null;
    Vector3 startPos;
    Vector3 endPos;
    Actor actor;
    int stepsTaken;
    int maxSteps;
    float boardWidth;
    float boardHeight;

    TreeNode<Vector3> _endNode;

    public RRT(Vector3 _startPos, Vector3 _endPos, Actor _actor, int _maxSteps, float _boardWidth, float _boardHeight)
    {
        startPos = _startPos;
        endPos = _endPos;
        actor = _actor;
        stepsTaken = 0;
        maxSteps = _maxSteps;
        boardWidth = _boardWidth;
        boardHeight = _boardHeight;

        Finished = Successful = false;
        tree = new TreeNode<Vector3>(_startPos);
    }

    public void NextStep()
    {
        int count = 0;
        bool success = false;
        while (!success)
        {
            success = false;
            Vector3 randPos = RRT_GetRandomState(endPos);
            TreeNode<Vector3> nearestNeighbor = RRT_GetNearestNeighbor(randPos, tree);

            //Select input to use
            //For now any input is valid

            //Determine new state
            Vector3 newPos = Vector3.positiveInfinity;
            if (RRT_StepTowards(nearestNeighbor.Value, randPos, ref newPos, actor))
            {
                success = true;
                TreeNode<Vector3> newNode = nearestNeighbor.AddChild(newPos);
                DrawLine(nearestNeighbor.Value, newNode.Value, Color.red, actor.transform, -1);
                if (Vector3.Distance(newNode.Value, endPos) < 0.1)
                {
                    _endNode = newNode;
                    Successful = true;
                    Finished = true;
                    return;
                }
            }
            count++;
            if(count > 10000)
            {
                Successful = false;
                Finished = false;
                return;
            }
        }
        stepsTaken++;
        if (stepsTaken >= maxSteps)
        {
            Successful = false;
            Finished = true;
        }
    }

    public List<Vector3> GetPath()
    {
        if (Finished && Successful)
        {
            return CreatePathFromTree(tree, _endNode);
        }
        else
        {
            return null;
        }
    }

    private Vector3 RRT_GetRandomState(Vector3 endPos)
    {
        if (Random.value < 0.2)
        {
            return endPos;
        }
        else
        {
            return new Vector3(Random.Range(-boardWidth / 2.0f, boardWidth / 2.0f), endPos.y, Random.Range(-boardHeight / 2.0f, boardHeight / 2.0f));
        }
    }

    private TreeNode<Vector3> RRT_GetNearestNeighbor(Vector3 randPos, TreeNode<Vector3> tree)
    {
        TreeNode<Vector3> nearestNeighbor = tree;
        float minDist = Vector3.Distance(randPos, nearestNeighbor.Value);
        _getNearestNeighbor(randPos, ref minDist, ref nearestNeighbor, tree);
        return nearestNeighbor;
    }

    private void _getNearestNeighbor(Vector3 randPos, ref float minDist, ref TreeNode<Vector3> nearestNeighbor, TreeNode<Vector3> node)
    {
        foreach (var child in node.Children)
        {
            float newDist = Vector3.Distance(randPos, child.Value);
            if (newDist < minDist)
            {
                minDist = newDist;
                nearestNeighbor = child;
            }
            _getNearestNeighbor(randPos, ref minDist, ref nearestNeighbor, child);
        }
    }

    private bool RRT_StepTowards(Vector3 start, Vector3 end, ref Vector3 newPos, Actor actor)
    {
        float epsilon = 3.0f;
        float dist = Mathf.Min(epsilon, Vector3.Distance(start, end));
        RaycastHit hit;
        if (Physics.Raycast(start, end - start, out hit, dist, 1 << LayerMask.NameToLayer("Obstacles")))
        {
            //Obstacle between start and end nodes
            newPos = Vector3.positiveInfinity;
            return false;
        }

        //Check that the actor doesn't hit any obstacles on the way from start to end
        actor.transform.position = start;
        newPos = start + (end - start).normalized * dist;
        int numSteps = 5;
        for (int step = 0; step < numSteps; ++step)
        {
            actor.transform.position = Vector3.Lerp(start, newPos, (float)step / (numSteps - 1));
            if (actor.HitsObstacle())
            {
                newPos = Vector3.positiveInfinity;
                return false;
            }
        }
        return true;
    }


    private List<Vector3> CreatePathFromTree(TreeNode<Vector3> tree, TreeNode<Vector3> endNode)
    {
        List<Vector3> path = new List<Vector3>();
        TreeNode<Vector3> cur = endNode;
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
