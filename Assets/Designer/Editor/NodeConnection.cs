using System;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	[Serializable]
	public class NodeConnection : ScriptableObject
	{
		[SerializeField]
		private NodeDesigner originatingNodeDesigner;

		[SerializeField]
		private NodeDesigner destinationNodeDesigner;

		[SerializeField]
		private NodeConnectionType nodeConnectionType;

		[SerializeField]
		private bool selected;

		[SerializeField]
		private float horizontalHeight;

		public NodeDesigner OriginatingNodeDesigner
		{
			get
			{
				return this.originatingNodeDesigner;
			}
			set
			{
				this.originatingNodeDesigner = value;
			}
		}

		public NodeDesigner DestinationNodeDesigner
		{
			get
			{
				return this.destinationNodeDesigner;
			}
			set
			{
				this.destinationNodeDesigner = value;
			}
		}

		public NodeConnectionType NodeConnectionType
		{
			get
			{
				return this.nodeConnectionType;
			}
			set
			{
				this.nodeConnectionType = value;
			}
		}

		public float HorizontalHeight
		{
			set
			{
				this.horizontalHeight = value;
			}
		}

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

		public void loadConnection(NodeDesigner nodeDesigner, NodeConnectionType nodeConnectionType)
		{
			this.originatingNodeDesigner = nodeDesigner;
			this.nodeConnectionType = nodeConnectionType;
			this.selected = false;
		}

		public void drawConnection(Vector2 offset, float graphZoom, bool disabled)
		{
			this.drawConnection(this.OriginatingNodeDesigner.getConnectionPosition(offset, NodeConnectionType.Outgoing), this.DestinationNodeDesigner.getConnectionPosition(offset, NodeConnectionType.Incoming), graphZoom, disabled);
		}

		public void drawConnection(Vector2 source, Vector2 destination, float graphZoom, bool disabled)
		{
			Color color = disabled ? new Color(0.7f, 0.7f, 0.7f) : Color.white;
			bool flag = this.destinationNodeDesigner != null && this.destinationNodeDesigner.Task != null && this.destinationNodeDesigner.Task.NodeData.PushTime != -1f && this.destinationNodeDesigner.Task.NodeData.PushTime >= this.destinationNodeDesigner.Task.NodeData.PopTime;
			float num = BehaviorDesignerPreferences.GetBool(BDPreferneces.FadeNodes) ? BehaviorDesignerUtility.NodeFadeDuration : 0.01f;
			if (this.selected)
			{
				if (disabled)
				{
					if (EditorGUIUtility.isProSkin)
					{
						color = new Color(0.1316f, 0.3212f, 0.4803f);
					}
					else
					{
						color = new Color(0.1701f, 0.3982f, 0.5873f);
					}
				}
				else if (EditorGUIUtility.isProSkin)
				{
					color = new Color(0.188f, 0.4588f, 0.6862f);
				}
				else
				{
					color = new Color(0.243f, 0.5686f, 0.839f);
				}
			}
			else if (flag)
			{
				if (EditorGUIUtility.isProSkin)
				{
					color = new Color(0f, 0.698f, 0.4f);
				}
				else
				{
					color = new Color(0f, 1f, 0.2784f);
				}
			}
			else if (num != 0f && this.destinationNodeDesigner != null && this.destinationNodeDesigner.Task != null && this.destinationNodeDesigner.Task.NodeData.PopTime != -1f && Time.realtimeSinceStartup- this.destinationNodeDesigner.Task.NodeData.PopTime < num)
			{
				float num2 = 1f - (Time.realtimeSinceStartup- this.destinationNodeDesigner.Task.NodeData.PopTime) / num;
				Color white = Color.white;
				if (EditorGUIUtility.isProSkin)
				{
					white = new Color(0f, 0.698f, 0.4f);
				}
				else
				{
					white = new Color(0f, 1f, 0.2784f);
				}
				color = Color.Lerp(Color.white, white, num2);
			}
			Handles.color=color;
			Vector3[] array = new Vector3[]
			{
				source,
				new Vector2(source.x, this.horizontalHeight),
				new Vector2(destination.x, this.horizontalHeight),
				destination
			};
			Handles.DrawAAPolyLine(BehaviorDesignerUtility.TaskConnectionTexture, 1f / graphZoom, array);
		}

		public bool contains(Vector2 point, Vector2 offset)
		{
			Vector2 center = this.originatingNodeDesigner.OutgoingConnectionRect(offset).center;
			Vector2 vector = new Vector2(center.x, this.horizontalHeight);
			float num = Mathf.Abs(point.x - center.x);
			if (num < (float)BehaviorDesignerUtility.LineSelectionThreshold && ((point.y >= center.y && point.y <= vector.y) || (point.y <= center.y && point.y >= vector.y)))
			{
				return true;
			}
			Rect rect = this.destinationNodeDesigner.IncomingConnectionRect(offset);
			Vector2 vector2 = new Vector2(rect.center.x, rect.y);
			Vector2 vector3 = new Vector2(vector2.x, this.horizontalHeight);
			num = Mathf.Abs(point.y - this.horizontalHeight);
			if (num < (float)BehaviorDesignerUtility.LineSelectionThreshold && ((point.x <= center.x && point.x >= vector3.x) || (point.x >= center.x && point.x <= vector3.x)))
			{
				return true;
			}
			num = Mathf.Abs(point.x - vector2.x);
			return num < (float)BehaviorDesignerUtility.LineSelectionThreshold && ((point.y >= vector2.y && point.y <= vector3.y) || (point.y <= vector2.y && point.y >= vector3.y));
		}
	}
}
