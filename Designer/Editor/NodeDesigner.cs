using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
    /// <summary>
    /// 节点
    /// </summary>
	[Serializable]
	public class NodeDesigner : ScriptableObject
	{
        /// <summary>
        /// 节点的任务
        /// </summary>
		[SerializeField]
		private Task mTask;
        /// <summary>
        /// 是否选中
        /// </summary>
		[SerializeField]
		private bool mSelected;

		private int mIdentifyUpdateCount = -1;

		[SerializeField]
		private bool mIsDirty;

		[SerializeField]
		private bool isParent;

		[SerializeField]
		private bool isEntryDisplay;

		[SerializeField]
		private bool showReferenceIcon;

		[SerializeField]
		private bool showHoverBar;

		[SerializeField]
		private string taskName = "";

		private bool prevRunningState;

		[SerializeField]
		private NodeDesigner parentNodeDesigner;

		[SerializeField]
		private List<NodeConnection> outgoingNodeConnections;

		public Task Task
		{
			get
			{
				return this.mTask;
			}
			set
			{
				this.mTask = value;
				this.init();
			}
		}

		public bool IsParent
		{
			get
			{
				return this.isParent;
			}
		}

		public bool IsEntryDisplay
		{
			get
			{
				return this.isEntryDisplay;
			}
		}

		public bool ShowReferenceIcon
		{
			set
			{
				this.showReferenceIcon = value;
			}
		}

		public bool ShowHoverBar
		{
			get
			{
				return this.showHoverBar;
			}
			set
			{
				this.showHoverBar = value;
			}
		}

		public NodeDesigner ParentNodeDesigner
		{
			get
			{
				return this.parentNodeDesigner;
			}
			set
			{
				this.parentNodeDesigner = value;
			}
		}

        /// <summary>
        /// 节点说有的连线
        /// </summary>
		public List<NodeConnection> OutgoingNodeConnections
		{
			get
			{
				return this.outgoingNodeConnections;
			}
		}

		public void select()
		{
			if (!this.isEntryDisplay)
			{
				this.mSelected = true;
			}
		}

		public void deselect()
		{
			this.mSelected = false;
		}

		public void markDirty()
		{
			this.mIsDirty = true;
		}

		public Rect IncomingConnectionRect(Vector2 offset)
		{
			Rect rect = this.rectangle(offset, false);
			return new Rect(rect.x + (rect.width - (float)BehaviorDesignerUtility.ConnectionWidth) / 2f, rect.y - (float)BehaviorDesignerUtility.TopConnectionHeight, (float)BehaviorDesignerUtility.ConnectionWidth, (float)BehaviorDesignerUtility.TopConnectionHeight);
		}

		public Rect OutgoingConnectionRect(Vector2 offset)
		{
			Rect rect = this.rectangle(offset, false);
			return new Rect(rect.x + (rect.width - (float)BehaviorDesignerUtility.ConnectionWidth) / 2f, rect.yMax, (float)BehaviorDesignerUtility.ConnectionWidth, (float)BehaviorDesignerUtility.BottomConnectionHeight);
		}

		public void OnEnable()
		{
            base.hideFlags = HideFlags.HideAndDontSave;
		}

		public bool OnInspectorUpdate()
		{
			bool result = false;
			if (this.isParent)
			{
				ParentTask parentTask = this.mTask as ParentTask;
				if (parentTask != null && parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						if (parentTask.Children[i] != null && parentTask.Children[i].NodeData.NodeDesigner != null && (parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner).OnInspectorUpdate())
						{
							result = true;
						}
					}
				}
			}
			return result;
		}

		public void loadTask(Task task, ref int id)
		{
			if (task == null)
			{
				return;
			}
			FieldInfo[] fields = task.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				if (fields[i].FieldType.IsSubclassOf(typeof(SharedVariable)))
				{
					SharedVariable sharedVariable = fields[i].GetValue(task) as SharedVariable;
					if (sharedVariable == null)
					{
						sharedVariable = (ScriptableObject.CreateInstance(fields[i].FieldType) as SharedVariable);
						fields[i].SetValue(task, sharedVariable);
					}
				}
			}
			this.mTask = task;
			this.mTask.ID = id++;
			this.mTask.NodeData.NodeDesigner = this;
			this.mTask.NodeData.initWatchedFields(this.mTask);
            this.mTask.hideFlags = (!AssetDatabase.GetAssetPath(task.Owner).Equals("")) ? HideFlags.DontSave : HideFlags.None;
			this.loadTaskIcon();
			this.init();
			if (this.isParent)
			{
				ParentTask parentTask = this.mTask as ParentTask;
				if (parentTask.Children != null)
				{
					for (int j = 0; j < parentTask.Children.Count; j++)
					{
						NodeDesigner nodeDesigner = ScriptableObject.CreateInstance<NodeDesigner>();
						nodeDesigner.loadTask(parentTask.Children[j], ref id);
						NodeConnection nodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
						nodeConnection.loadConnection(this, NodeConnectionType.Fixed);
						this.addChildNode(nodeDesigner, nodeConnection, true, j);
					}
				}
				this.mIsDirty = true;
			}
		}

		public void loadNode(Task task, BehaviorSource behaviorSource, Vector2 position, ref int id)
		{
			this.mTask = task;
			this.mTask.Owner = (behaviorSource.Owner as Behavior);
			FieldInfo[] fields = this.mTask.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				if (fields[i].FieldType.IsSubclassOf(typeof(SharedVariable)))
				{
					SharedVariable sharedVariable = fields[i].GetValue(task) as SharedVariable;
					if (sharedVariable == null)
					{
						sharedVariable = (ScriptableObject.CreateInstance(fields[i].FieldType) as SharedVariable);
						fields[i].SetValue(this.mTask, sharedVariable);
					}
				}
			}
			this.mTask.ID = id++;
			this.mTask.NodeData = new NodeData();
			this.mTask.NodeData.Position = position;
			this.mTask.NodeData.NodeDesigner = this;
			this.loadTaskIcon();
			this.init();
			this.mTask.NodeData.FriendlyName = this.taskName;
		}

		private void loadTaskIcon()
		{
			this.mTask.NodeData.Icon = null;
			TaskIconAttribute[] array;
			if ((array = (this.mTask.GetType().GetCustomAttributes(typeof(TaskIconAttribute), false) as TaskIconAttribute[])).Length > 0)
			{
				this.mTask.NodeData.Icon = BehaviorDesignerUtility.LoadIcon(array[0].IconPath, null);
			}
			if (this.mTask.NodeData.Icon == null)
			{
				string iconName;
				if (this.mTask.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Action)))
				{
					iconName = "{SkinColor}ActionIcon.png";
				}
				else if (this.mTask.GetType().IsSubclassOf(typeof(Conditional)))
				{
					iconName = "{SkinColor}ConditionalIcon.png";
				}
				else if (this.mTask.GetType().IsSubclassOf(typeof(Composite)))
				{
					iconName = "{SkinColor}CompositeIcon.png";
				}
				else if (this.mTask.GetType().IsSubclassOf(typeof(Decorator)))
				{
					iconName = "{SkinColor}DecoratorIcon.png";
				}
				else
				{
					iconName = "{SkinColor}EntryIcon.png";
				}
				this.mTask.NodeData.Icon = BehaviorDesignerUtility.LoadIcon(iconName, null);
			}
		}

		private void init()
		{
			this.taskName = BehaviorDesignerUtility.SplitCamelCase(this.mTask.GetType().Name.ToString());
			this.isParent = this.mTask.GetType().IsSubclassOf(typeof(ParentTask));
			if (this.isParent)
			{
				this.outgoingNodeConnections = new List<NodeConnection>();
			}
		}

		public void makeEntryDisplay()
		{
			this.isEntryDisplay = (this.isParent = true);
			this.mTask.NodeData.FriendlyName = (this.taskName = "Entry");
			this.outgoingNodeConnections = new List<NodeConnection>();
		}

		private Rect rectangle(Vector2 offset, bool includeConnections)
		{
			Rect result = this.rectangle(offset);
			if (includeConnections)
			{
				if (!this.isEntryDisplay)
				{
					result.yMin=result.yMin - (float)BehaviorDesignerUtility.TopConnectionHeight;
				}
				if (this.isParent)
				{
					result.yMax=result.yMax + (float)BehaviorDesignerUtility.BottomConnectionHeight;
				}
			}
			return result;
		}

		private Rect rectangle(Vector2 offset)
		{
			if (this.mTask == null)
			{
				return default(Rect);
			}
			float num = BehaviorDesignerUtility.TaskTitleGUIStyle.CalcSize(new GUIContent(this.ToString())).x + (float)BehaviorDesignerUtility.TextPadding;
			if (!this.isParent)
			{
				float num2;
				float num3;
                BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(this.mTask.NodeData.Comment), out num2, out num3);
				num3 += (float)BehaviorDesignerUtility.TextPadding;
				num = ((num > num3) ? num : num3);
			}
			num = Mathf.Min((float)BehaviorDesignerUtility.MaxWidth, Mathf.Max((float)BehaviorDesignerUtility.MinWidth, num));
			return new Rect(this.mTask.NodeData.Position.x + offset.x - num / 2f, this.mTask.NodeData.Position.y + offset.y, num, (float)(BehaviorDesignerUtility.IconAreaHeight + BehaviorDesignerUtility.TitleHeight));
		}

		public bool drawNode(Vector2 offset, bool drawSelected, bool disabled)
		{
			if (drawSelected != this.mSelected)
			{
				return false;
			}
			Rect rect = this.rectangle(offset, false);
			bool flag = (this.mTask.NodeData.PushTime != -1f && this.mTask.NodeData.PushTime >= this.mTask.NodeData.PopTime) ||
                        (this.isEntryDisplay && this.outgoingNodeConnections.Count > 0 && this.outgoingNodeConnections[0].DestinationNodeDesigner.Task.NodeData.PushTime != -1f);
			bool flag2 = this.mIdentifyUpdateCount != -1;
			float num = BehaviorDesignerPreferences.GetBool(BDPreferneces.FadeNodes) ? BehaviorDesignerUtility.NodeFadeDuration : 0.01f;
			float num2 = 0f;
			if (flag2)
			{
				if (BehaviorDesignerUtility.MaxIdentifyUpdateCount - this.mIdentifyUpdateCount < BehaviorDesignerUtility.IdentifyUpdateFadeTime)
				{
					num2 = (float)(BehaviorDesignerUtility.MaxIdentifyUpdateCount - this.mIdentifyUpdateCount) / (float)BehaviorDesignerUtility.IdentifyUpdateFadeTime;
				}
				else
				{
					num2 = 1f;
				}
				if (this.mIdentifyUpdateCount != -1)
				{
					this.mIdentifyUpdateCount++;
					if (this.mIdentifyUpdateCount > BehaviorDesignerUtility.MaxIdentifyUpdateCount)
					{
						this.mIdentifyUpdateCount = -1;
					}
				}
			}
			else if (flag)
			{
				num2 = 1f;
			}
			else if ((this.mTask.NodeData.PopTime != -1f && num != 0f && Time.realtimeSinceStartup - this.mTask.NodeData.PopTime < num) || (this.isEntryDisplay && this.outgoingNodeConnections.Count > 0 && this.outgoingNodeConnections[0].DestinationNodeDesigner.Task.NodeData.PopTime != -1f && Time.realtimeSinceStartup - this.outgoingNodeConnections[0].DestinationNodeDesigner.Task.NodeData.PopTime < num))
			{
				if (this.isEntryDisplay)
				{
					num2 = 1f - (Time.realtimeSinceStartup - this.outgoingNodeConnections[0].DestinationNodeDesigner.Task.NodeData.PopTime) / num;
				}
				else
				{
					num2 = 1f - (Time.realtimeSinceStartup - this.mTask.NodeData.PopTime) / num;
				}
			}
			if (!this.isEntryDisplay && !this.prevRunningState && flag)
			{
				this.parentNodeDesigner.bringConnectionToFront(this);
			}
			this.prevRunningState = flag;
			if (num2 != 1f)
			{
				GUI.color=(disabled || this.mTask.NodeData.Disabled) ? new Color(0.7f, 0.7f, 0.7f) : Color.white;
				if (!this.isEntryDisplay)
				{
					GUI.DrawTexture(
                        new Rect(
                        rect.x + (rect.width - (float)BehaviorDesignerUtility.ConnectionWidth) / 2f, 
                        rect.y - (float)BehaviorDesignerUtility.TopConnectionHeight - (float)BehaviorDesignerUtility.TaskBackgroundShadowSize + 3f,
                        (float)BehaviorDesignerUtility.ConnectionWidth, (float)(BehaviorDesignerUtility.TopConnectionHeight + BehaviorDesignerUtility.TaskBackgroundShadowSize)
                        )
                        , 
                        BehaviorDesignerUtility.TaskConnectionTopTexture, ScaleMode.ScaleToFit);
				}
				if (this.isParent)
				{
					GUI.DrawTexture(
                        new Rect(rect.x + (rect.width - (float)BehaviorDesignerUtility.ConnectionWidth) / 2f, rect.yMax - 3f, (float)BehaviorDesignerUtility.ConnectionWidth, (float)(BehaviorDesignerUtility.BottomConnectionHeight + BehaviorDesignerUtility.TaskBackgroundShadowSize)),
                        BehaviorDesignerUtility.TaskConnectionBottomTexture, ScaleMode.ScaleToFit);
				}
				GUI.Label(rect, "", this.mSelected ? BehaviorDesignerUtility.TaskSelectedGUIStyle : BehaviorDesignerUtility.TaskGUIStyle);
				this.drawExecutionStatus(rect);
				GUI.DrawTexture(new Rect(rect.x + (rect.width - (float)BehaviorDesignerUtility.IconBorderSize) / 2f, rect.y + (float)((BehaviorDesignerUtility.IconAreaHeight - BehaviorDesignerUtility.IconBorderSize) / 2) + 2f, (float)BehaviorDesignerUtility.IconBorderSize, (float)BehaviorDesignerUtility.IconBorderSize), BehaviorDesignerUtility.TaskBorderTexture);
			}
			if (num2 > 0f)
			{
				GUIStyle gUIStyle;
				Texture2D texture2D;
				if (this.mIdentifyUpdateCount != -1)
				{
					if (this.mSelected)
					{
						gUIStyle = BehaviorDesignerUtility.TaskIdentifySelectedGUIStyle;
					}
					else
					{
						gUIStyle = BehaviorDesignerUtility.TaskIdentifyGUIStyle;
					}
					texture2D = BehaviorDesignerUtility.TaskBorderIdentifyTexture;
				}
				else
				{
					if (this.mSelected)
					{
						gUIStyle = BehaviorDesignerUtility.TaskRunningSelectedGUIStyle;
					}
					else
					{
						gUIStyle = BehaviorDesignerUtility.TaskRunningGUIStyle;
					}
					texture2D = BehaviorDesignerUtility.TaskBorderRunningTexture;
				}
				Color color = (disabled || this.mTask.NodeData.Disabled) ? new Color(0.7f, 0.7f, 0.7f) : Color.white;
				color.a = num2;
				GUI.color=color;
				if (!this.isEntryDisplay)
				{
					Texture2D texture2D2;
					if (this.mIdentifyUpdateCount != -1)
					{
						texture2D2 = BehaviorDesignerUtility.TaskConnectionIdentifyTopTexture;
					}
					else
					{
						texture2D2 = BehaviorDesignerUtility.TaskConnectionRunningTopTexture;
					}
					GUI.DrawTexture(new Rect(rect.x + (rect.width - (float)BehaviorDesignerUtility.ConnectionWidth) / 2f, 
                        rect.y - (float)BehaviorDesignerUtility.TopConnectionHeight - (float)BehaviorDesignerUtility.TaskBackgroundShadowSize + 3f,
                        (float)BehaviorDesignerUtility.ConnectionWidth, (float)(BehaviorDesignerUtility.TopConnectionHeight + BehaviorDesignerUtility.TaskBackgroundShadowSize)), texture2D2,
                         ScaleMode.ScaleToFit);
				}
				if (this.isParent)
				{
					Texture2D texture2D3;
					if (this.mIdentifyUpdateCount != -1)
					{
						texture2D3 = BehaviorDesignerUtility.TaskConnectionIdentifyBottomTexture;
					}
					else
					{
						texture2D3 = BehaviorDesignerUtility.TaskConnectionRunningBottomTexture;
					}
					GUI.DrawTexture(new Rect(rect.x + (rect.width - (float)BehaviorDesignerUtility.ConnectionWidth) / 2f, rect.yMax - 3f, 
                        (float)BehaviorDesignerUtility.ConnectionWidth,
                        (float)(BehaviorDesignerUtility.BottomConnectionHeight + BehaviorDesignerUtility.TaskBackgroundShadowSize)), texture2D3, ScaleMode.ScaleToFit);
				}
				GUI.Label(rect, "", gUIStyle);
				this.drawExecutionStatus(rect);
				GUI.DrawTexture(new Rect(rect.x + (rect.width - (float)BehaviorDesignerUtility.IconBorderSize) / 2f, rect.y + (float)((BehaviorDesignerUtility.IconAreaHeight - BehaviorDesignerUtility.IconBorderSize) / 2) + 2f, (float)BehaviorDesignerUtility.IconBorderSize, (float)BehaviorDesignerUtility.IconBorderSize), texture2D);
				GUI.color=Color.white;
			}
			if (this.mTask.NodeData.Collapsed)
			{
				GUI.DrawTexture(new Rect(rect.x + (rect.width - (float)BehaviorDesignerUtility.TaskConnectionCollapsedWidth) / 2f + 1f, rect.yMax + 2f, (float)BehaviorDesignerUtility.TaskConnectionCollapsedWidth, (float)BehaviorDesignerUtility.TaskConnectionCollapsedHeight), BehaviorDesignerUtility.TaskConnectionCollapsedTexture);
			}
            if (this.mTask.NodeData.Icon != null)
            {
                GUI.DrawTexture
                    (
                    new Rect(rect.x + (rect.width - (float)BehaviorDesignerUtility.IconSize) / 2f, rect.y + (float)((BehaviorDesignerUtility.IconAreaHeight - BehaviorDesignerUtility.IconSize) / 2) + 2f,
                    (float)BehaviorDesignerUtility.IconSize, (float)BehaviorDesignerUtility.IconSize),
                    this.mTask.NodeData.Icon
                    );
            }
			GUI.Label(new Rect(rect.x, rect.yMax - (float)BehaviorDesignerUtility.TitleHeight - 1f, rect.width, (float)BehaviorDesignerUtility.TitleHeight), this.ToString(), BehaviorDesignerUtility.TaskTitleGUIStyle);
			if (this.mTask.NodeData.IsBreakpoint)
			{
				GUI.DrawTexture(new Rect(rect.xMax - 20f, rect.y + 4f, 14f, 14f), BehaviorDesignerUtility.BreakpointTexture);
			}
			if (this.showReferenceIcon)
			{
				GUI.DrawTexture(new Rect(rect.x + 6f, rect.y + 4f, 14f, 14f), BehaviorDesignerUtility.ReferencedTexture);
			}
			GUI.color=Color.white;
			if (this.showHoverBar)
			{
                GUI.DrawTexture(new Rect(rect.x - 1f, rect.y - 17f, 14f, 14f), this.mTask.NodeData.Disabled ? BehaviorDesignerUtility.EnableTaskTexture : BehaviorDesignerUtility.DisableTaskTexture, ScaleMode.ScaleToFit);
				if (this.isParent)
				{
                    GUI.DrawTexture(new Rect(rect.x + 15f, rect.y - 17f, 14f, 14f), this.mTask.NodeData.Collapsed ? BehaviorDesignerUtility.ExpandTaskTexture : BehaviorDesignerUtility.CollapseTaskTexture, ScaleMode.ScaleToFit);
				}
			}
			return num2 > 0f;
		}

		private void drawExecutionStatus(Rect nodeRect)
		{
			if (this.mTask.NodeData.ExecutionStatus == TaskStatus.Success)
			{
				GUI.DrawTexture(new Rect(nodeRect.xMax - 35f, nodeRect.yMax - 33f, 35f, 31f), BehaviorDesignerUtility.ExecutionSuccessTexture);
				return;
			}
			if (this.mTask.NodeData.ExecutionStatus == TaskStatus.Failure)
			{
				GUI.DrawTexture(new Rect(nodeRect.xMax - 37f, nodeRect.yMax - 38f, 35f, 36f), BehaviorDesignerUtility.ExecutionFailureTexture);
			}
		}

		public void drawNodeConnection(Vector2 offset, float graphZoom, bool disabled)
		{
			if (this.mIsDirty)
			{
				this.determineConnectionHorizontalHeight(this.rectangle(offset, false), offset);
				this.mIsDirty = false;
			}
			if (this.isParent)
			{
				for (int i = 0; i < this.outgoingNodeConnections.Count; i++)
				{
					this.outgoingNodeConnections[i].drawConnection(offset, graphZoom, disabled);
				}
			}
		}

		public void drawNodeComment(Vector2 offset)
		{
			if (this.mTask.NodeData.Comment.Equals("") && (this.mTask.NodeData.WatchedFields == null || this.mTask.NodeData.WatchedFields.Count == 0))
			{
				return;
			}
			Rect rect = this.rectangle(offset, false);
			bool flag = false;
			if (this.mTask.NodeData.WatchedFields != null && this.mTask.NodeData.WatchedFields.Count > 0)
			{
				string text = "";
				string text2 = "";
				for (int i = 0; i < this.mTask.NodeData.WatchedFields.Count; i++)
				{
					FieldInfo fieldInfo = this.mTask.NodeData.WatchedFields[i];
					text = text + BehaviorDesignerUtility.SplitCamelCase(fieldInfo.Name) + ": \n";
					text2 = text2 + ((fieldInfo.GetValue(this.mTask) != null) ? fieldInfo.GetValue(this.mTask).ToString() : "null") + "\n";
				}
				float num;
				float num2;
                BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(text), out num, out num2);
				float num3;
                BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(text2), out num, out num3);
				float num4 = num2;
				float num5 = num3;
				float num6 = Mathf.Min((float)BehaviorDesignerUtility.MaxWidth, num2 + num3 + (float)BehaviorDesignerUtility.TextPadding);
				if (num6 == (float)BehaviorDesignerUtility.MaxWidth)
				{
					num4 = num2 / (num2 + num3) * (float)BehaviorDesignerUtility.MaxWidth;
					num5 = num3 / (num2 + num3) * (float)BehaviorDesignerUtility.MaxWidth;
				}
				GUI.Box(new Rect(rect.xMax + 4f, rect.y, num6 + 8f, rect.height), "");
				GUI.Label(new Rect(rect.xMax + 6f, rect.y + 4f, num4, rect.height - 8f), text, BehaviorDesignerUtility.TaskCommentRightAlignGUIStyle);
				GUI.Label(new Rect(rect.xMax + 6f + num4, rect.y + 4f, num5, rect.height - 8f), text2, BehaviorDesignerUtility.TaskCommentLeftAlignGUIStyle);
				flag = true;
			}
			if (!this.mTask.NodeData.Comment.Equals(""))
			{
				if (this.isParent)
				{
					float num7;
					float num8;
                    BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(this.mTask.NodeData.Comment), out num7, out num8);
					float num9 = Mathf.Min((float)BehaviorDesignerUtility.MaxWidth, num8 + (float)BehaviorDesignerUtility.TextPadding);
					if (flag)
					{
						GUI.Box(new Rect(rect.xMin - 12f - num9, rect.y, num9 + 8f, rect.height), "");
						GUI.Label(new Rect(rect.xMin - 6f - num9, rect.y + 4f, num9, rect.height - 8f), this.mTask.NodeData.Comment, BehaviorDesignerUtility.TaskCommentGUIStyle);
						return;
					}
					GUI.Box(new Rect(rect.xMax + 4f, rect.y, num9 + 8f, rect.height), "");
					GUI.Label(new Rect(rect.xMax + 6f, rect.y + 4f, num9, rect.height - 8f), this.mTask.NodeData.Comment, BehaviorDesignerUtility.TaskCommentGUIStyle);
					return;
				}
				else
				{
					float num10 = Mathf.Min((float)BehaviorDesignerUtility.MaxCommentHeight, BehaviorDesignerUtility.TaskCommentGUIStyle.CalcHeight(new GUIContent(this.mTask.NodeData.Comment), rect.width - 4f));
					GUI.Box(new Rect(rect.x, rect.yMax + 4f, rect.width, num10 + 4f), "");
					GUI.Label(new Rect(rect.x, rect.yMax + 4f, rect.width - 4f, num10), this.mTask.NodeData.Comment, BehaviorDesignerUtility.TaskCommentGUIStyle);
				}
			}
		}

		public bool contains(Vector2 point, Vector2 offset, bool includeConnections)
		{
			return this.rectangle(offset, includeConnections).Contains(point);
		}

		public NodeConnection nodeConnectionRectContains(Vector2 point, Vector2 offset)
		{
			bool flag;
			if ((flag = this.IncomingConnectionRect(offset).Contains(point)) || (this.isParent && this.OutgoingConnectionRect(offset).Contains(point)))
			{
				NodeConnection nodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
				nodeConnection.loadConnection(this, flag ? NodeConnectionType.Incoming : NodeConnectionType.Outgoing);
				return nodeConnection;
			}
			return null;
		}

		public void connectionContains(Vector2 point, Vector2 offset, ref List<NodeConnection> nodeConnections)
		{
			if (this.outgoingNodeConnections == null || this.isEntryDisplay)
			{
				return;
			}
			for (int i = 0; i < this.outgoingNodeConnections.Count; i++)
			{
				if (this.outgoingNodeConnections[i].contains(point, offset))
				{
					nodeConnections.Add(this.outgoingNodeConnections[i]);
				}
			}
		}

		private void determineConnectionHorizontalHeight(Rect nodeRect, Vector2 offset)
		{
			if (this.isParent)
			{
				float num = 3.40282347E+38f;
				float num2 = num;
				for (int i = 0; i < this.outgoingNodeConnections.Count; i++)
				{
					Rect rect = this.outgoingNodeConnections[i].DestinationNodeDesigner.rectangle(offset, false);
					if (rect.y < num)
					{
						num = rect.y;
						num2 = rect.y;
					}
				}
				num = num * 0.75f + nodeRect.yMax * 0.25f;
				if (num < nodeRect.yMax + 15f)
				{
					num = nodeRect.yMax + 15f;
				}
				else if (num > num2 - 15f)
				{
					num = num2 - 15f;
				}
				for (int j = 0; j < this.outgoingNodeConnections.Count; j++)
				{
					this.outgoingNodeConnections[j].HorizontalHeight = num;
				}
			}
		}

		public Vector2 getConnectionPosition(Vector2 offset, NodeConnectionType connectionType)
		{
			Vector2 result;
			if (connectionType == NodeConnectionType.Incoming)
			{
				Rect rect = this.IncomingConnectionRect(offset);
				result = new Vector2(rect.center.x, rect.y + (float)(BehaviorDesignerUtility.TopConnectionHeight / 2));
			}
			else
			{
				Rect rect2 = this.OutgoingConnectionRect(offset);
				result = new Vector2(rect2.center.x, rect2.yMax - (float)(BehaviorDesignerUtility.BottomConnectionHeight / 2));
			}
			return result;
		}

		public bool hoverBarAreaContains(Vector2 point, Vector2 offset)
		{
            Rect rect = this.rectangle(offset, false);
            rect.y=rect.y - 24f;
            return rect.Contains(point);
		}

		public bool hoverBarButtonClick(Vector2 point, Vector2 offset, ref bool collapsedButtonClicked)
		{
			Rect rect = this.rectangle(offset, false);
			Rect rect2 = new Rect(rect.x - 1f, rect.y - 17f, 14f, 14f);
			Rect rect3 = rect2;
			bool flag = false;
			if (rect2.Contains(point))
			{
				this.mTask.NodeData.Disabled = !this.mTask.NodeData.Disabled;
				flag = true;
			}
			if (!flag && this.isParent)
			{
				Rect rect4 = new Rect(rect.x + 15f, rect.y - 17f, 14f, 14f);
				rect3.xMax=rect4.xMax;
				if (rect4.Contains(point))
				{
					this.mTask.NodeData.Collapsed = !this.mTask.NodeData.Collapsed;
					collapsedButtonClicked = true;
					flag = true;
				}
			}
			if (!flag && rect3.Contains(point))
			{
				flag = true;
			}
			return flag;
		}

		public bool intersects(Rect rect, Vector2 offset)
		{
			Rect rect2 = this.rectangle(offset, false);
			return rect2.xMin < rect.xMax && rect2.xMax > rect.xMin && rect2.yMin < rect.yMax && rect2.yMax > rect.yMin;
		}

		public void movePosition(Vector2 delta)
		{
			Vector2 vector = this.mTask.NodeData.Position;
			vector += delta;
			this.mTask.NodeData.Position = vector;
			if (this.parentNodeDesigner != null)
			{
				this.parentNodeDesigner.markDirty();
			}
		}

		public void addChildNode(NodeDesigner childNodeDesigner, NodeConnection nodeConnection, bool replaceNode)
		{
			this.addChildNode(childNodeDesigner, nodeConnection, replaceNode, -1);
		}

		public void addChildNode(NodeDesigner childNodeDesigner, NodeConnection nodeConnection, bool replaceNode, int replaceNodeIndex)
		{
			if (replaceNode)
			{
				ParentTask parentTask = this.mTask as ParentTask;
				parentTask.Children[replaceNodeIndex] = childNodeDesigner.Task;
			}
			else if (!this.isEntryDisplay)
			{
				ParentTask parentTask2 = this.mTask as ParentTask;
				int num = 0;
				if (parentTask2.Children != null)
				{
					num = 0;
					while (num < parentTask2.Children.Count && childNodeDesigner.Task.NodeData.Position.x >= parentTask2.Children[num].NodeData.Position.x)
					{
						num++;
					}
				}
				parentTask2.AddChild(childNodeDesigner.Task, num);
			}
			childNodeDesigner.ParentNodeDesigner = this;
			nodeConnection.DestinationNodeDesigner = childNodeDesigner;
			nodeConnection.NodeConnectionType = NodeConnectionType.Fixed;
			if (!nodeConnection.OriginatingNodeDesigner.Equals(this))
			{
				nodeConnection.OriginatingNodeDesigner = this;
			}
			this.outgoingNodeConnections.Add(nodeConnection);
			this.mIsDirty = true;
		}

		public void removeChildNode(NodeDesigner childNodeDesigner)
		{
			if (!this.isEntryDisplay)
			{
				ParentTask parentTask = this.mTask as ParentTask;
				parentTask.Children.Remove(childNodeDesigner.Task);
			}
			for (int i = 0; i < this.outgoingNodeConnections.Count; i++)
			{
				NodeConnection nodeConnection = this.outgoingNodeConnections[i];
				if (nodeConnection.DestinationNodeDesigner.Equals(childNodeDesigner) || nodeConnection.OriginatingNodeDesigner.Equals(childNodeDesigner))
				{
					this.outgoingNodeConnections.RemoveAt(i);
					break;
				}
			}
			childNodeDesigner.ParentNodeDesigner = null;
			this.mIsDirty = true;
		}

		public void setID(ref int id)
		{
			this.mTask.ID = id++;
			if (this.isParent)
			{
				ParentTask parentTask = this.mTask as ParentTask;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner).setID(ref id);
					}
				}
			}
		}

		public int childIndexForTask(Task childTask)
		{
			if (this.isParent)
			{
				ParentTask parentTask = this.mTask as ParentTask;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						if (parentTask.Children[i].Equals(childTask))
						{
							return i;
						}
					}
				}
			}
			return -1;
		}

		public NodeDesigner nodeDesignerForChildIndex(int index)
		{
			if (index < 0)
			{
				return null;
			}
			if (this.isParent)
			{
				ParentTask parentTask = this.mTask as ParentTask;
				if (parentTask.Children != null)
				{
					if (index >= parentTask.Children.Count || parentTask.Children[index] == null)
					{
						return null;
					}
					return parentTask.Children[index].NodeData.NodeDesigner as NodeDesigner;
				}
			}
			return null;
		}

		public void moveChildNode(int index, bool decreaseIndex)
		{
			int index2 = index + (decreaseIndex ? -1 : 1);
			ParentTask parentTask = this.mTask as ParentTask;
			Task value = parentTask.Children[index];
			parentTask.Children[index] = parentTask.Children[index2];
			parentTask.Children[index2] = value;
		}

		private void bringConnectionToFront(NodeDesigner nodeDesigner)
		{
			for (int i = 0; i < this.outgoingNodeConnections.Count; i++)
			{
				if (this.outgoingNodeConnections[i].DestinationNodeDesigner.Equals(nodeDesigner))
				{
					NodeConnection value = this.outgoingNodeConnections[i];
					this.outgoingNodeConnections[i] = this.outgoingNodeConnections[this.outgoingNodeConnections.Count - 1];
					this.outgoingNodeConnections[this.outgoingNodeConnections.Count - 1] = value;
					return;
				}
			}
		}

		public void toggleBreakpoint()
		{
			this.mTask.NodeData.IsBreakpoint = !this.Task.NodeData.IsBreakpoint;
		}

		public void toggleEnableState()
		{
			this.mTask.NodeData.Disabled = !this.Task.NodeData.Disabled;
		}

		public bool isDisabled()
		{
			return this.mTask.NodeData.Disabled || (this.parentNodeDesigner != null && this.parentNodeDesigner.isDisabled());
		}

		public bool toggleCollapseState()
		{
			this.mTask.NodeData.Collapsed = !this.Task.NodeData.Collapsed;
			return this.mTask.NodeData.Collapsed;
		}

		public void identifyNode()
		{
			this.mIdentifyUpdateCount = 0;
		}

		public bool hasParent(NodeDesigner nodeDesigner)
		{
			return !(this.parentNodeDesigner == null) && (this.parentNodeDesigner.Equals(nodeDesigner) || this.parentNodeDesigner.hasParent(nodeDesigner));
		}

		public override bool Equals(object obj)
		{
			return object.ReferenceEquals(this, obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			if (this.mTask == null)
			{
				return "";
			}
			if (!this.mTask.NodeData.FriendlyName.Equals(""))
			{
				return this.mTask.NodeData.FriendlyName;
			}
			return this.taskName;
		}
	}
}
