using System;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 连线
/// </summary>
[Serializable]
public class NodeConnection : ScriptableObject
{
    private bool selected;

    public Task OriginatingNodeDesigner;

    public Task DestinationNodeDesigner;

    public TaskOutLink Originating;


    public NodeConnectionType NodeConnectionType;

    public float HorizontalHeight;

    public void select()
    {
        this.selected = true;
    }

    public void deselect()
    {
        this.selected = false;
    }

    public void OnEnable()
    {
        base.hideFlags = HideFlags.HideAndDontSave;
    }

    public void loadConnection(Task nodeDesigner, NodeConnectionType nodeConnectionType)
    {
        OriginatingNodeDesigner = nodeDesigner;
        NodeConnectionType = nodeConnectionType;
        this.selected = false;
    }



    public void drawConnection(Vector2 source, Vector2 destination, float graphZoom, bool disabled)
    {
        Vector3[] array = new Vector3[]
			{
				source,
				destination
			};
        Handles.DrawAAPolyLine(1f / graphZoom, array);
    }

    //public bool contains(Vector2 point, Vector2 offset)
    //{
    //    Vector2 center = OriginatingNodeDesigner.OutgoingConnectionRect(offset).center;
    //    Vector2 vector = new Vector2(center.x, HorizontalHeight);
    //    float num = Mathf.Abs(point.x - center.x);
    //    if (num < (float)BehaviorDesignerUtility.LineSelectionThreshold && ((point.y >= center.y && point.y <= vector.y) || (point.y <= center.y && point.y >= vector.y)))
    //    {
    //        return true;
    //    }
    //    Rect rect = DestinationNodeDesigner.IncomingConnectionRect(offset);
    //    Vector2 vector2 = new Vector2(rect.center.x, rect.y);
    //    Vector2 vector3 = new Vector2(vector2.x, HorizontalHeight);
    //    num = Mathf.Abs(point.y - HorizontalHeight);
    //    if (num < (float)BehaviorDesignerUtility.LineSelectionThreshold && ((point.x <= center.x && point.x >= vector3.x) || (point.x >= center.x && point.x <= vector3.x)))
    //    {
    //        return true;
    //    }
    //    num = Mathf.Abs(point.x - vector2.x);
    //    return num < (float)BehaviorDesignerUtility.LineSelectionThreshold && ((point.y >= vector2.y && point.y <= vector3.y) || (point.y <= vector2.y && point.y >= vector3.y));
    //}
}
public enum NodeConnectionType
{
    Incoming,
    Outgoing,
    Fixed
}