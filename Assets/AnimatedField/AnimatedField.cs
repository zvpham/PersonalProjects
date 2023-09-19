using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class AnimatedField : MonoBehaviour
{
    public CreatedField createdField;
    public float createdFieldQuickness = 1;
    public bool nonStandardDuration = false;

    public Grid<CreatedObjectStatus> grid;

    public bool affectFlying;

    public GameManager gameManager;

    public struct Node
    {
        public Vector3 position;
        public Vector3 direction;
        public int priority;
        public int blastValue;
        public float blastSpeed;
        public float affectedTimeFlow;
        public GameObject createdObject;

        public Node(Vector3 position, Vector3 direction, int priority, int blastValue, float blastSpeed, float affectedTimeFlow, GameObject createdobject)
        {
            this.position = position;
            this.direction = direction;
            this.priority = priority;
            this.blastValue = blastValue;
            this.blastSpeed = blastSpeed;
            this.affectedTimeFlow = affectedTimeFlow;
            this.createdObject = createdobject;
        }

        public override string ToString()
        {
            return position.ToString() + " " + priority + " " + blastSpeed;
        }
    }

    public enum NodeState
    {
        wall,
        unit,
        emptySpace
    }

    abstract public void SetParameters(Vector3 startPosition, Vector3 direction, float angle, GameObject createdObject, int range,
        List<Vector3> markerLocations, int initialConeBlastValue, int maxObstacleBlastValueAbsorbtion, int maxUnitBlastValueAbsorbtion,
        float secEmenateSpeed = .035f, bool isAffectFlying = true, bool ignoreWalls = true);

    abstract public void GetAnimation();

    abstract public void Activate();
}
