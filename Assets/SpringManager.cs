using UnityEngine;

public class SpringManager : MonoBehaviour
{
    [SerializeField]
    private SpringJoint2D Joint;
    public static SpringManager Instance;
    
    private void Awake()
    {
        Instance = this;
    }

    public void ConnectToTarget(Rigidbody2D body, Vector2 target)
    {
        Joint.anchor = body.position;
        Joint.connectedBody = body;
        Joint.connectedAnchor = target;
        Joint.enabled = true;
    }

    public void Detach()
    {
        Joint.anchor = Vector2.zero;
        Joint.connectedBody = null;
        Joint.connectedAnchor = Vector2.zero;
        Joint.enabled = false;
    }

    public SpringJoint2D GetSpring()
    {
        return Joint;
    }
}
