using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	[Serializable]
	public class GraphDesigner : ScriptableObject
	{
		[SerializeField]
		private int mNextTaskID;

		private List<int> mNodeSelectedID = new List<int>();

		[SerializeField]
		private int[] mPrevNodeSelectedID;

        ///// <summary>
        ///// 根节点
        ///// </summary>
        //public NodeDesigner RootNode;

        public List<NodeDesigner> DetachedNodes=new List<NodeDesigner>();

        public List<NodeDesigner> SelectedNodes=new List<NodeDesigner>();
        /// <summary>
        /// 获取焦点的节点
        /// </summary>
        public NodeDesigner HoverNode;

        public NodeConnection ActiveNodeConnection;

        public List<NodeConnection> SelectedNodeConnections=new List<NodeConnection>();

		public void OnEnable()
		{
            base.hideFlags = HideFlags.HideAndDontSave;
		}

		public bool OnInspectorUpdate()
		{
            //if (this.mEntryTask == null)
            //{
            //    return false;
            //}
            //bool result = this.mEntryTask.OnInspectorUpdate();

            //if (RootNode == null) { return false; }
            //bool result = RootNode.OnInspectorUpdate();


            if (DetachedNodes == null || DetachedNodes.Count <= 0) { return false; }
            bool result = false;
			for (int i = 0; i < DetachedNodes.Count; i++)
			{
				if (DetachedNodes[i].OnInspectorUpdate())
				{
					result = true;
				}
			}
			return result;
		}

        /// <summary>
        /// 寻找范围内的节点
        /// </summary>
        /// <param name="point"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
		public NodeDesigner nodeAt(Vector2 point, Vector2 offset)
		{
            if (DetachedNodes == null || DetachedNodes.Count <= 0)
			{
				return null;
			}
			for (int i = 0; i < SelectedNodes.Count; i++)
			{
				if (SelectedNodes[i].contains(point, offset, false))
				{
					return SelectedNodes[i];
				}
			}
			NodeDesigner result;
			for (int j = DetachedNodes.Count - 1; j > -1; j--)
			{
				if (DetachedNodes[j] != null && (result = this.nodeChildrenAt(DetachedNodes[j], point, offset)) != null)
				{
					return result;
				}
			}
            //if (RootNode != null && (result = this.nodeChildrenAt(RootNode, point, offset)) != null)
            //{
            //    return result;
            //}
            //if (RootNode.contains(point, offset, true))
            //{
            //    return RootNode;
            //}
			return null;
		}

		public NodeDesigner nodeChildrenAt(NodeDesigner nodeDesigner, Vector2 point, Vector2 offset)
		{
			if (nodeDesigner.contains(point, offset, true))
			{
				return nodeDesigner;
			}
            //if (nodeDesigner.IsParent)
            //{
                Task parentTask = nodeDesigner.Task;
				if (!parentTask.NodeData.Collapsed && parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						NodeDesigner result;
						if (parentTask.Children[i] != null && (result = this.nodeChildrenAt(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner, point, offset)) != null)
						{
							return result;
						}
					}
				}
            //}
			return null;
		}

		public List<NodeDesigner> nodesAt(Rect rect, Vector2 offset)
		{
			List<NodeDesigner> list = new List<NodeDesigner>();
            //if (RootNode != null)
            //{
            //    this.nodesChildrenAt(RootNode, rect, offset, ref list);
            //}
			for (int i = 0; i < DetachedNodes.Count; i++)
			{
				this.nodesChildrenAt(DetachedNodes[i], rect, offset, ref list);
			}
			if (list.Count <= 0)
			{
				return null;
			}
			return list;
		}

		public void nodesChildrenAt(NodeDesigner nodeDesigner, Rect rect, Vector2 offset, ref List<NodeDesigner> nodes)
		{
			if (nodeDesigner.intersects(rect, offset))
			{
				nodes.Add(nodeDesigner);
			}
            //if (nodeDesigner.IsParent)
            //{
				Task parentTask = nodeDesigner.Task;
				if (!parentTask.NodeData.Collapsed && parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						if (parentTask.Children[i] != null)
						{
							this.nodesChildrenAt(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner, rect, offset, ref nodes);
						}
					}
				}
            //}
		}

		public bool isSelected(NodeDesigner nodeDesigner)
		{
			return SelectedNodes.Contains(nodeDesigner);
		}

		public void select(NodeDesigner nodeDesigner)
		{
			this.select(nodeDesigner, true);
		}

		public void select(NodeDesigner nodeDesigner, bool addHash)
		{
			if (SelectedNodes.Count == 1)
			{
				this.indicateReferencedTasks(SelectedNodes[0].Task, false);
			}
			SelectedNodes.Add(nodeDesigner);
			if (addHash)
			{
				this.mNodeSelectedID.Add(nodeDesigner.Task.ID);
			}
			nodeDesigner.select();
			if (SelectedNodes.Count == 1)
			{
				this.indicateReferencedTasks(SelectedNodes[0].Task, true);
			}
		}

		public void deselect(NodeDesigner nodeDesigner)
		{
			SelectedNodes.Remove(nodeDesigner);
			this.mNodeSelectedID.Remove(nodeDesigner.Task.ID);
			nodeDesigner.deselect();
			this.indicateReferencedTasks(nodeDesigner.Task, false);
		}

		public void deselectAllExcept(NodeDesigner nodeDesigner)
		{
			for (int i = SelectedNodes.Count - 1; i >= 0; i--)
			{
				if (!SelectedNodes[i].Equals(nodeDesigner))
				{
					SelectedNodes.RemoveAt(i);
					this.mNodeSelectedID.RemoveAt(i);
					SelectedNodes[i].deselect();
				}
			}
			this.indicateReferencedTasks(nodeDesigner.Task, false);
		}

		public void clearNodeSelection()
		{
			if (SelectedNodes.Count == 1)
			{
				this.indicateReferencedTasks(SelectedNodes[0].Task, false);
			}
			for (int i = 0; i < SelectedNodes.Count; i++)
			{
				SelectedNodes[i].deselect();
			}
			SelectedNodes.Clear();
			this.mNodeSelectedID.Clear();
		}

		public void deselectWithParent(NodeDesigner nodeDesigner)
		{
			for (int i = SelectedNodes.Count - 1; i >= 0; i--)
			{
				if (SelectedNodes[i].hasParent(nodeDesigner))
				{
					this.deselect(SelectedNodes[i]);
				}
			}
		}

		public void hover(NodeDesigner nodeDesigner)
		{
			if (!nodeDesigner.ShowHoverBar)
			{
				nodeDesigner.ShowHoverBar = true;
				this.HoverNode = nodeDesigner;
			}
		}

		public void clearHover()
		{
			if (this.HoverNode)
			{
				this.HoverNode.ShowHoverBar = false;
				this.HoverNode = null;
			}
		}

		private void indicateReferencedTasks(Task task, bool indicate)
		{
			List<Task> referencedTasks = TaskInspector.GetReferencedTasks(task);
			if (referencedTasks != null && referencedTasks.Count > 0)
			{
				for (int i = 0; i < referencedTasks.Count; i++)
				{
					if (referencedTasks[i] != null && referencedTasks[i].NodeData != null)
					{
						NodeDesigner nodeDesigner = referencedTasks[i].NodeData.NodeDesigner as NodeDesigner;
						if (nodeDesigner != null)
						{
							nodeDesigner.ShowReferenceIcon = indicate;
						}
					}
				}
			}
		}
        /// <summary>
        /// 拖动节点
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="dragChildren"></param>
        /// <param name="hasDragged"></param>
        /// <returns></returns>
		public bool dragSelectedNodes(Vector2 delta, bool dragChildren, bool hasDragged)
		{
			if (SelectedNodes.Count == 0)
			{
				return false;
			}
			bool flag = SelectedNodes.Count == 1;
			for (int i = 0; i < SelectedNodes.Count; i++)
			{
				this.dragTask(SelectedNodes[i], delta, dragChildren, hasDragged);
			}
            //if (flag && dragChildren && SelectedNodes[0].IsRootDisplay && RootNode != null)
            //{
            //    this.dragTask(RootNode, delta, dragChildren, hasDragged);
            //}
			return true;
		}

		public void dragTask(NodeDesigner nodeDesigner, Vector2 delta, bool dragChildren, bool hasDragged)
		{
			if (!hasDragged)
			{
				BehaviorUndo.RegisterUndo("Drag", nodeDesigner.Task, true, false);
			}
			nodeDesigner.movePosition(delta);
			if (nodeDesigner.ParentNodeDesigner != null)
			{
				int num = nodeDesigner.ParentNodeDesigner.childIndexForTask(nodeDesigner.Task);
				if (num != -1)
				{
					int index = num - 1;
					bool flag = false;
					NodeDesigner nodeDesigner2 = nodeDesigner.ParentNodeDesigner.nodeDesignerForChildIndex(index);
					if (nodeDesigner2 != null && nodeDesigner.Task.NodeData.Position.x < nodeDesigner2.Task.NodeData.Position.x)
					{
						nodeDesigner.ParentNodeDesigner.moveChildNode(num, true);
						flag = true;
					}
					if (!flag)
					{
						index = num + 1;
						nodeDesigner2 = nodeDesigner.ParentNodeDesigner.nodeDesignerForChildIndex(index);
						if (nodeDesigner2 != null && nodeDesigner.Task.NodeData.Position.x > nodeDesigner2.Task.NodeData.Position.x)
						{
							nodeDesigner.ParentNodeDesigner.moveChildNode(num, false);
						}
					}
				}
			}
			if (dragChildren)
			{
                Task parentTask = nodeDesigner.Task;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						NodeDesigner nodeDesigner3;
						if (!this.isSelected(nodeDesigner3 = (parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner)))
						{
							this.dragTask(nodeDesigner3, delta, dragChildren, hasDragged);
						}
					}
				}
			}
		}

		public bool nodeCanOriginateConnection(NodeDesigner nodeDesigner, NodeConnection connection)
		{
			return !nodeDesigner.IsRootDisplay || (nodeDesigner.IsRootDisplay && connection.NodeConnectionType == NodeConnectionType.Outgoing);
		}

		public bool nodeCanAcceptConnection(NodeDesigner nodeDesigner, NodeConnection connection)
		{
            //if ((!nodeDesigner.IsRootDisplay || connection.NodeConnectionType != NodeConnectionType.Incoming) && (nodeDesigner.IsRootDisplay || (!nodeDesigner.IsParent && (connection.NodeConnectionType != NodeConnectionType.Outgoing))))
            //{
            //    return false;
            //}
            //if (nodeDesigner.IsRootDisplay || connection.OriginatingNodeDesigner.IsRootDisplay)
            //{
            //    return true;
            //}
			HashSet<NodeDesigner> hashSet = new HashSet<NodeDesigner>();
			NodeDesigner nodeDesigner2 = (connection.NodeConnectionType == NodeConnectionType.Outgoing) ? nodeDesigner : connection.OriginatingNodeDesigner;
			NodeDesigner item = (connection.NodeConnectionType == NodeConnectionType.Outgoing) ? connection.OriginatingNodeDesigner : nodeDesigner;
			return !this.cycleExists(nodeDesigner2, ref hashSet) && !hashSet.Contains(item);
		}

		private bool cycleExists(NodeDesigner nodeDesigner, ref HashSet<NodeDesigner> set)
		{
			if (set.Contains(nodeDesigner))
			{
				return true;
			}
			set.Add(nodeDesigner);
            //if (nodeDesigner.IsParent)
            //{
                Task parentTask = nodeDesigner.Task;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						if (this.cycleExists(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner, ref set))
						{
							return true;
						}
					}
				}
            //}
			return false;
		}

		public void connectNodes(BehaviorSource behaviorSource, NodeDesigner nodeDesigner)
		{
			NodeConnection nodeConnection = ActiveNodeConnection;
			ActiveNodeConnection = null;
			if (nodeConnection != null && !nodeConnection.OriginatingNodeDesigner.Equals(nodeDesigner))
			{
				NodeDesigner originatingNodeDesigner = nodeConnection.OriginatingNodeDesigner;
				BehaviorUndo.RegisterUndo("Connection", this, true, true);
                //BehaviorUndo.RegisterUndo("Connection", behaviorSource.Owner.GetObject(), false, true);
				BehaviorUndo.RegisterUndo("Connection", nodeDesigner, false, true);
				BehaviorUndo.RegisterUndo("Connection", originatingNodeDesigner, false, true);
				if (nodeConnection.NodeConnectionType == NodeConnectionType.Outgoing)
				{
					BehaviorUndo.RegisterUndo("Connection", originatingNodeDesigner.Task, false, true);
					this.removeParentConnection(nodeDesigner);
					this.checkForLastConnectionRemoval(originatingNodeDesigner);
					originatingNodeDesigner.addChildNode(nodeDesigner, nodeConnection, false);
				}
				else
				{
					BehaviorUndo.RegisterUndo("Connection", nodeDesigner.Task, false, true);
					this.removeParentConnection(originatingNodeDesigner);
					this.checkForLastConnectionRemoval(nodeDesigner);
					nodeDesigner.addChildNode(originatingNodeDesigner, nodeConnection, false);
				}
                //if (nodeConnection.OriginatingNodeDesigner.IsRootDisplay)
                //{
                //    RootNode = nodeConnection.DestinationNodeDesigner;
                //}
				DetachedNodes.Remove(nodeConnection.DestinationNodeDesigner);
			}
		}

		private void removeParentConnection(NodeDesigner nodeDesigner)
		{
			if (nodeDesigner.ParentNodeDesigner != null)
			{
				NodeDesigner parentNodeDesigner = nodeDesigner.ParentNodeDesigner;
				NodeConnection nodeConnection = null;
				for (int i = 0; i < parentNodeDesigner.OutgoingNodeConnections.Count; i++)
				{
					if (parentNodeDesigner.OutgoingNodeConnections[i].DestinationNodeDesigner.Equals(nodeDesigner))
					{
						nodeConnection = parentNodeDesigner.OutgoingNodeConnections[i];
						break;
					}
				}
				if (nodeConnection != null)
				{
					this.removeConnection(nodeConnection, "Connect");
				}
			}
		}

        private void checkForLastConnectionRemoval(NodeDesigner nodeDesigner)
        {
            //if (nodeDesigner.IsRootDisplay)
            //{
            //    if (nodeDesigner.OutgoingNodeConnections.Count == 1)
            //    {
            //        this.removeConnection(nodeDesigner.OutgoingNodeConnections[0], "Connect");
            //        return;
            //    }
            //}
            //else
            //{
            Task parentTask = nodeDesigner.Task;
            if (parentTask.Children != null)
            {
                NodeConnection nodeConnection = null;
                for (int i = 0; i < nodeDesigner.OutgoingNodeConnections.Count; i++)
                {
                    if (nodeDesigner.OutgoingNodeConnections[i].DestinationNodeDesigner.Equals(parentTask.Children[parentTask.Children.Count - 1].NodeData.NodeDesigner as NodeDesigner))
                    {
                        nodeConnection = nodeDesigner.OutgoingNodeConnections[i];
                        break;
                    }
                }
                if (nodeConnection != null)
                {
                    this.removeConnection(nodeConnection, "Connect");
                }
            }
            //}
        }

        public void nodeConnectionsAt(Vector2 point, Vector2 offset, ref List<NodeConnection> nodeConnections)
        {
            //if (this.mEntryTask == null)
            //{
            //    return;
            //}
            //this.nodeChildrenConnectionsAt(this.mEntryTask, point, offset, ref nodeConnections);

            if (DetachedNodes == null || DetachedNodes.Count <= 0) { return; }

            //this.nodeChildrenConnectionsAt(RootNode, point, offset, ref nodeConnections);

            for (int i = 0; i < DetachedNodes.Count; i++)
            {
                this.nodeChildrenConnectionsAt(DetachedNodes[i], point, offset, ref nodeConnections);
            }
        }

		public void nodeChildrenConnectionsAt(NodeDesigner nodeDesigner, Vector2 point, Vector2 offset, ref List<NodeConnection> nodeConnections)
		{
			nodeDesigner.connectionContains(point, offset, ref nodeConnections);
            //if (nodeDesigner.IsParent)
            //{
                Task parentTask = nodeDesigner.Task;
				if (parentTask != null && parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						if (parentTask.Children[i] != null)
						{
							this.nodeChildrenConnectionsAt(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner, point, offset, ref nodeConnections);
						}
					}
				}
            //}
		}

		public void removeConnection(NodeConnection nodeConnection, string undoName)
		{
			BehaviorUndo.RegisterUndo(undoName, nodeConnection.OriginatingNodeDesigner, false, true);
			BehaviorUndo.RegisterUndo(undoName, nodeConnection.OriginatingNodeDesigner.Task, false, true);
			BehaviorUndo.RegisterUndo(undoName, nodeConnection.DestinationNodeDesigner, false, true);
			DetachedNodes.Add(nodeConnection.DestinationNodeDesigner);
			nodeConnection.OriginatingNodeDesigner.removeChildNode(nodeConnection.DestinationNodeDesigner);
            //if (nodeConnection.OriginatingNodeDesigner.IsRootDisplay)
            //{
            //    RootNode = null;
            //}
		}

		public void moveChildNode(NodeDesigner nodeDesigner, int index, bool decreaseIndex)
		{
			nodeDesigner.moveChildNode(index, decreaseIndex);
		}

		public bool isSelected(NodeConnection nodeConnection)
		{
			for (int i = 0; i < SelectedNodeConnections.Count; i++)
			{
				if (SelectedNodeConnections[i].Equals(nodeConnection))
				{
					return true;
				}
			}
			return false;
		}

		public void select(NodeConnection nodeConnection)
		{
			SelectedNodeConnections.Add(nodeConnection);
			nodeConnection.select();
		}

		public void deselect(NodeConnection nodeConnection)
		{
			SelectedNodeConnections.Remove(nodeConnection);
			nodeConnection.deselect();
		}

		public void clearConnectionSelection()
		{
			for (int i = 0; i < SelectedNodeConnections.Count; i++)
			{
				SelectedNodeConnections[i].deselect();
			}
			SelectedNodeConnections.Clear();
		}

		public void graphDirty()
		{
            //if (this.mEntryTask == null)
            //{
            //    return;
            //}
            //this.mEntryTask.markDirty();

            if (DetachedNodes == null || DetachedNodes.Count <= 0)
            {
                return;
            }
            //RootNode.markDirty();//???????
            //if (RootNode != null)
            //{
            //    this.markTaskDirty(RootNode);
            //}
			for (int i = DetachedNodes.Count - 1; i > -1; i--)
			{
				this.markTaskDirty(DetachedNodes[i]);
			}
		}

		private void markTaskDirty(NodeDesigner nodeDesigner)
		{
			nodeDesigner.markDirty();
            //if (nodeDesigner.IsParent)
            //{
				Task parentTask = nodeDesigner.Task;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						if (parentTask.Children[i] != null)
						{
							this.markTaskDirty(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner);
						}
					}
				}
            //}
		}

		public void selectAll()
		{
			for (int i = SelectedNodes.Count - 1; i > -1; i--)
			{
				this.deselect(SelectedNodes[i]);
			}
            //if (RootNode != null)
            //{
            //    this.selectAll(RootNode);
            //}
			for (int j = DetachedNodes.Count - 1; j > -1; j--)
			{
				this.selectAll(DetachedNodes[j]);
			}
		}

        private void selectAll(NodeDesigner nodeDesigner)
        {
            this.select(nodeDesigner);

            Task parentTask = nodeDesigner.Task;
            if (parentTask.Children != null)
            {
                for (int i = 0; i < parentTask.Children.Count; i++)
                {
                    this.selectAll(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner);
                }
            }

        }

		public List<TaskSerializer> copy()
		{
			List<TaskSerializer> list = new List<TaskSerializer>();
			for (int i = 0; i < SelectedNodes.Count; i++)
			{
				TaskSerializer taskSerializer;
				if ((taskSerializer = TaskCopier.CopySerialized(SelectedNodes[i].Task)) != null)
				{
                    //if (SelectedNodes[i].IsParent)
                    //{
                        Task parentTask = SelectedNodes[i].Task;
						if (parentTask.Children != null)
						{
							List<int> list2 = new List<int>();
							for (int j = 0; j < parentTask.Children.Count; j++)
							{
								int item;
								if ((item = SelectedNodes.IndexOf(parentTask.Children[j].NodeData.NodeDesigner as NodeDesigner)) != -1)
								{
									list2.Add(item);
								}
							}
							taskSerializer.childrenIndex = list2;
						}
                    //}
					list.Add(taskSerializer);
				}
			}
			if (list.Count <= 0)
			{
				return null;
			}
			return list;
		}

		public bool paste(BehaviorSource behaviorSource, List<TaskSerializer> copiedTasks)
		{
			if (copiedTasks == null || copiedTasks.Count == 0)
			{
				return false;
			}
			this.clearNodeSelection();
			this.clearConnectionSelection();
			List<NodeDesigner> list = new List<NodeDesigner>();
			for (int i = 0; i < copiedTasks.Count; i++)
			{
				TaskSerializer taskSerializer = copiedTasks[i];
				NodeDesigner nodeDesigner = this.addNode(behaviorSource, taskSerializer.taskType, taskSerializer.position);
				for (int j = 0; j < taskSerializer.fieldInfo.Length; j++)
				{
					if (taskSerializer.fieldInfo[j].FieldType.IsSubclassOf(typeof(SharedVariable)))
					{
						if (taskSerializer.fieldInfo[j].FieldType.IsSubclassOf(typeof(SharedVariable)))
						{
							SharedVariable sharedVariable = taskSerializer.fieldValue[j] as SharedVariable;
							if (sharedVariable != null)
							{
								SharedVariable sharedVariable2 = behaviorSource.GetVariable(sharedVariable.name);
								if (sharedVariable2 == null)
								{
									sharedVariable2 = (ScriptableObject.CreateInstance(sharedVariable.GetType()) as SharedVariable);
									sharedVariable2.SetValue(sharedVariable.GetValue());
									sharedVariable2.IsShared = sharedVariable.IsShared;
								}
								taskSerializer.fieldInfo[j].SetValue(nodeDesigner.Task, sharedVariable2);
							}
						}
					}
					else
					{
						taskSerializer.fieldInfo[j].SetValue(nodeDesigner.Task, taskSerializer.fieldValue[j]);
					}
				}
				nodeDesigner.Task.NodeData.FriendlyName = taskSerializer.friendlyName;
				nodeDesigner.Task.NodeData.Comment = taskSerializer.comment;
				list.Add(nodeDesigner);
				this.select(nodeDesigner);
			}
			for (int k = 0; k < copiedTasks.Count; k++)
			{
				TaskSerializer taskSerializer2 = copiedTasks[k];
				if (taskSerializer2.childrenIndex != null)
				{
					for (int l = 0; l < taskSerializer2.childrenIndex.Count; l++)
					{
						NodeDesigner nodeDesigner2 = list[k];
						NodeConnection nodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
						nodeConnection.loadConnection(nodeDesigner2, NodeConnectionType.Outgoing);
						nodeDesigner2.addChildNode(list[taskSerializer2.childrenIndex[l]], nodeConnection, false);
						DetachedNodes.Remove(list[taskSerializer2.childrenIndex[l]]);
					}
				}
			}
			this.save(behaviorSource);
			return true;
		}

		public void identifyNode(NodeDesigner nodeDesigner)
		{
			nodeDesigner.identifyNode();
		}
    
        /// <summary>
        /// 是否有根节点
        /// </summary>
        /// <returns></returns>
		public bool hasRootNode()
		{
			if (DetachedNodes == null || DetachedNodes.Count<=0)
            {
                return false;
            }
            return true;
		}

        /// <summary>
        /// 根节点位置
        /// </summary>
        /// <returns></returns>
		public Vector2 rootNodePosition()
		{
            return DetachedNodes[0].Task.NodeData.Position;
           // return RootNode.Task.NodeData.Position;
		}

		private void loadNodeSelection(NodeDesigner nodeDesigner)
		{
			if (nodeDesigner == null)
			{
				return;
			}
			if (this.mNodeSelectedID != null && this.mNodeSelectedID.Contains(nodeDesigner.Task.ID))
			{
				this.select(nodeDesigner, false);
			}
            //if (nodeDesigner.IsParent)
            //{
                Task parentTask = nodeDesigner.Task;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						if (parentTask.Children[i] != null)
						{
							this.loadNodeSelection(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner);
						}
					}
				}
            //}
		}

		public void clear(bool saveSelectedNodes)
		{
			if (saveSelectedNodes)
			{
				this.mPrevNodeSelectedID = this.mNodeSelectedID.ToArray();
			}
			else
			{
				this.mPrevNodeSelectedID = null;
			}
			this.mNodeSelectedID.Clear();
			SelectedNodes.Clear();
			SelectedNodeConnections.Clear();
            //this.mEntryTask = null;
            //RootNode = null;
			DetachedNodes = new List<NodeDesigner>();
		}


        #region 绘制节点和连线
        /// <summary>
        ///  绘制节点
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <param name="offset"></param>
        /// <param name="graphZoom"></param>
        /// <returns></returns>
        public bool drawNodes(Vector2 mousePosition, Vector2 offset, float graphZoom)
        {
            bool result = false;
            if (DetachedNodes == null || DetachedNodes.Count<=0) { return false; }

            ////绘制根节点
            //this.drawNodeConnectionChildren(RootNode, offset, graphZoom, RootNode.Task.NodeData.Disabled);//绘制根节点的连线
            //result = this.drawNodeChildren(RootNode, offset, RootNode.Task.NodeData.Disabled);//绘制节点

            //绘制正在连接的线（鼠标拖动的连线）
            if (mousePosition != new Vector2(-1f, -1f) && ActiveNodeConnection != null)
            {
                ActiveNodeConnection.HorizontalHeight = (ActiveNodeConnection.OriginatingNodeDesigner.getConnectionPosition(offset, ActiveNodeConnection.NodeConnectionType).y + mousePosition.y) / 2f;
                ActiveNodeConnection.drawConnection(
                    ActiveNodeConnection.OriginatingNodeDesigner.getConnectionPosition(offset, ActiveNodeConnection.NodeConnectionType),
                    mousePosition,
                    graphZoom,
                    ActiveNodeConnection.NodeConnectionType == NodeConnectionType.Outgoing && ActiveNodeConnection.OriginatingNodeDesigner.isDisabled());
            }
            //绘制未连线的节点
            for (int i = 0; i < DetachedNodes.Count; i++)
            {
                //绘制节点一下所有节点的连线
                this.drawNodeConnectionChildren(DetachedNodes[i], offset, graphZoom, DetachedNodes[i].Task.NodeData.Disabled);

                //绘制自己和自己的子节点
                if (this.drawNodeChildren(DetachedNodes[i], offset, DetachedNodes[i].Task.NodeData.Disabled))
                {
                    result = true;
                }

                this.drawNodeCommentChildren(DetachedNodes[i], offset);
            }
            //绘制选中的节点
            for (int l = 0; l < SelectedNodes.Count; l++)
            {
                if (SelectedNodes[l].drawNode(offset, true, SelectedNodes[l].isDisabled()))
                {
                    result = true;
                }
            }
            //绘制选中的连线
            for (int j = 0; j < SelectedNodeConnections.Count; j++)
            {
                SelectedNodeConnections[j].drawConnection(offset, graphZoom, SelectedNodeConnections[j].OriginatingNodeDesigner.isDisabled());
            }
            return result;
        }

        private bool drawNodeChildren(NodeDesigner nodeDesigner, Vector2 offset, bool disabledNode)
        {
            if (nodeDesigner == null)
            {
                return false;
            }
            bool result = false;
            if (nodeDesigner.drawNode(offset, false, disabledNode))
            {
                result = true;
            }
            //if (nodeDesigner.IsParent)
            //{
                Task parentTask = nodeDesigner.Task;
                if (!parentTask.NodeData.Collapsed && parentTask.Children != null)
                {
                    for (int i = parentTask.Children.Count - 1; i > -1; i--)
                    {
                        if (parentTask.Children[i] != null && this.drawNodeChildren(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner, offset, parentTask.NodeData.Disabled || disabledNode))
                        {
                            result = true;
                        }
                    }
                }
            //}
            return result;
        }
        /// <summary>
        /// 绘制节点到儿子节点的连线
        /// </summary>
        /// <param name="nodeDesigner"></param>
        /// <param name="offset"></param>
        /// <param name="graphZoom"></param>
        /// <param name="disabledNode"></param>
        private void drawNodeConnectionChildren(NodeDesigner nodeDesigner, Vector2 offset, float graphZoom, bool disabledNode)
        {
            if (nodeDesigner == null)
            {
                return;
            }

            //if (nodeDesigner.taskName == "Entry Task") { Debug.Log("isRootDisplay:" + nodeDesigner.Task.NodeData.Collapsed); }


            if (!nodeDesigner.Task.NodeData.Collapsed)
            {
                nodeDesigner.drawNodeConnection(offset, graphZoom, nodeDesigner.Task.NodeData.Disabled || disabledNode);
                //if (nodeDesigner.IsParent)
                //{
                    Task parentTask = nodeDesigner.Task;
                    if (parentTask.Children != null)
                    {
                        for (int i = 0; i < parentTask.Children.Count; i++)
                        {
                            if (parentTask.Children[i] != null)
                            {
                                this.drawNodeConnectionChildren(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner, offset, graphZoom, parentTask.NodeData.Disabled || disabledNode);
                            }
                        }
                    }
                //}
            }
        }

        private void drawNodeCommentChildren(NodeDesigner nodeDesigner, Vector2 offset)
        {
            if (nodeDesigner == null)
            {
                return;
            }
            nodeDesigner.drawNodeComment(offset);
            //if (nodeDesigner.IsParent)
            //{
                Task parentTask = nodeDesigner.Task;
                if (!parentTask.NodeData.Collapsed && parentTask.Children != null)
                {
                    for (int i = 0; i < parentTask.Children.Count; i++)
                    {
                        if (parentTask.Children[i] != null)
                        {
                            this.drawNodeCommentChildren(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner, offset);
                        }
                    }
                }
            //}
        }

        #endregion

        #region 序列化
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="behaviorSource"></param>
        public void save(BehaviorSource behaviorSource)
        {
            if (DetachedNodes == null || DetachedNodes.Count <= 0)
            {
                return;
            }
            List<Task> list = new List<Task>();
            for (int i = 0; i < DetachedNodes.Count; i++)
            {
                list.Add(DetachedNodes[i].Task);
            }
            behaviorSource.save(list);
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="behaviorSource"></param>
        /// <param name="loadPrevBehavior"></param>
        /// <param name="nodePosition"></param>
        /// <returns></returns>
        public bool Load(BehaviorSource behaviorSource, bool loadPrevBehavior, Vector2 nodePosition)
        {
            //behaviorSource.CheckForJSONSerialization(behaviorSource.Owner != null && (PrefabUtility.GetPrefabType(behaviorSource.Owner.GetObject()) == PrefabType.Prefab || PrefabUtility.GetPrefabType(behaviorSource.Owner.GetObject()) == PrefabType.PrefabInstance));
            //if (behaviorSource.DetachedTasks == null)
            //{
            //    this.clear(false);
            //    return false;
            //}
            //if (loadPrevBehavior)
            //{
            //    SelectedNodes.Clear();
            //    SelectedNodeConnections.Clear();
            //    if (this.mPrevNodeSelectedID != null)
            //    {
            //        for (int i = 0; i < this.mPrevNodeSelectedID.Length; i++)
            //        {
            //            this.mNodeSelectedID.Add(this.mPrevNodeSelectedID[i]);
            //        }
            //        this.mPrevNodeSelectedID = null;
            //    }
            //}
            //else
            //{
            //    this.clear(false);
            //}
            //this.mNextTaskID = 0;
            ////RootNode = null;
            //DetachedNodes.Clear();
            //Task task;
            //List<Task> list;
            //behaviorSource.load(out list);
            //int num = BehaviorDesignerUtility.JSONTaskCount(behaviorSource.Serialization);
            //if (num > 0)
            //{
            //    int num2 = BehaviorDesignerUtility.TaskCount(behaviorSource);
            //    if (num2 != num)
            //    {
            //        behaviorSource.CheckForJSONSerialization(true);
            //        behaviorSource.load(out list);
            //    }
            //}

            ////if (task != null)
            ////{//加载根节点
            ////    RootNode = ScriptableObject.CreateInstance<NodeDesigner>();
            ////    RootNode.ResetNodeConnection();//重置连接
            ////    RootNode.loadTask(task, ref this.mNextTaskID);//加载节点和节点的子节点
            ////    RootNode.RootDisplay();//采用根节点显示的方式显示

            ////    this.loadNodeSelection(RootNode);
            ////}
            //if (list != null)
            //{
            //    for (int j = 0; j < list.Count; j++)
            //    {
            //        NodeDesigner nodeDesigner = ScriptableObject.CreateInstance<NodeDesigner>();
            //        nodeDesigner.loadTask(list[j], ref this.mNextTaskID);
            //        DetachedNodes.Add(nodeDesigner);
            //        this.loadNodeSelection(nodeDesigner);
            //    }
            //}
            return true;
        }
        public bool Load(BehaviorSource data)
        {
            if (data.DetachedTasks == null)
            {
                clear(false);
                return false ;
            }
            DetachedNodes.Clear();
            List<Task> list=data.GetTasks();
            if (list != null)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    NodeDesigner nodeDesigner = ScriptableObject.CreateInstance<NodeDesigner>();
                    nodeDesigner.loadTask(list[j], ref this.mNextTaskID);
                    DetachedNodes.Add(nodeDesigner);
                    loadNodeSelection(nodeDesigner);
                }
            }
            return true;

        }
        #endregion

        #region 添加/删除 节点
        /// <summary>
        /// 添加一个节点
        /// </summary>
        /// <param name="behaviorSource"></param>
        /// <param name="type"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public NodeDesigner addNode(BehaviorSource behaviorSource, Type type, Vector2 position)
        {
            BehaviorUndo.RegisterUndo("Add Task", this, true, true);

            Task task;
            //if (RootNode == null)
            //{//添加跟节点
            //    task = (ScriptableObject.CreateInstance("EntryTask") as Task);
            //    RootNode = ScriptableObject.CreateInstance<NodeDesigner>();
            //    RootNode.loadNode(task, behaviorSource, new Vector2(position.x, position.y - 120f), ref this.mNextTaskID);
            //    RootNode.RootDisplay();
            //}

            task = (ScriptableObject.CreateInstance(type) as Task);
            if (task == null)
            {
                Debug.LogError(string.Format("Unable to create task of type {0}. Is the class name the same as the file name?", type));
                return null;
            }

            //编辑器创建一个ui节点
            NodeDesigner nodeDesigner = ScriptableObject.CreateInstance<NodeDesigner>();
            nodeDesigner.loadNode(task, behaviorSource, position, ref this.mNextTaskID);

            //if (RootNode.OutgoingNodeConnections.Count == 0)
            //{
            //    //创建一个连接
            //    ActiveNodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
            //    ActiveNodeConnection.loadConnection(RootNode, NodeConnectionType.Outgoing);
            //    this.connectNodes(behaviorSource, nodeDesigner);
            //}
            //else
            //{
            DetachedNodes.Add(nodeDesigner);
            //}
            return nodeDesigner;
        }
        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="behaviorSource"></param>
        /// <returns></returns>
        public bool delete(BehaviorSource behaviorSource)
        {
            BehaviorUndo.RegisterUndo("Delete", this, true, true);
            //BehaviorUndo.RegisterUndo("Delete", behaviorSource.Owner.GetObject(), false, true);
            bool flag = false;
            if (SelectedNodeConnections != null)
            {
                for (int i = 0; i < SelectedNodeConnections.Count; i++)
                {
                    this.removeConnection(SelectedNodeConnections[i], "Delete");
                }
                SelectedNodeConnections.Clear();
                flag = true;
            }
            if (SelectedNodes != null)
            {
                //删除选中的节点
                for (int j = 0; j < SelectedNodes.Count; j++)
                {
                    this.removeNode(SelectedNodes[j]);
                }
                SelectedNodes.Clear();
                flag = true;
            }
            if (flag)
            {
                this.mNextTaskID = 0;
                //this.mEntryTask.setID(ref this.mNextTaskID);//???
                //if (RootNode != null)
                //{
                //    RootNode.setID(ref this.mNextTaskID);
                //}
                for (int k = 0; k < DetachedNodes.Count; k++)
                {
                    DetachedNodes[k].setID(ref this.mNextTaskID);
                }
                this.save(behaviorSource);
            }
            return flag;
        }
        /// <summary>
        /// 删除一个节点
        /// </summary>
        /// <param name="nodeDesigner"></param>
        public void removeNode(NodeDesigner nodeDesigner)
        {
            if (nodeDesigner.IsRootDisplay)
            {
                return;
            }
            //if (nodeDesigner.IsParent)
            //{
                for (int i = 0; i < nodeDesigner.OutgoingNodeConnections.Count; i++)
                {
                    NodeDesigner destinationNodeDesigner = nodeDesigner.OutgoingNodeConnections[i].DestinationNodeDesigner;
                    BehaviorUndo.RegisterUndo("Delete", destinationNodeDesigner, false, true);
                    DetachedNodes.Add(destinationNodeDesigner);
                    destinationNodeDesigner.ParentNodeDesigner = null;
                }
            //}
            if (nodeDesigner.ParentNodeDesigner != null)
            {
                BehaviorUndo.RegisterUndo("Delete", nodeDesigner.ParentNodeDesigner, false, true);
                BehaviorUndo.RegisterCompleteUndo("Delete", nodeDesigner.ParentNodeDesigner.Task);
                nodeDesigner.ParentNodeDesigner.removeChildNode(nodeDesigner);
            }
            //if (RootNode != null && RootNode.Equals(nodeDesigner))
            //{
            //    RootNode.removeChildNode(nodeDesigner);
            //    RootNode = null;
            //}
            bool flag = false;
            bool flag2 = false;
            FieldInfo[] fields = nodeDesigner.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int j = 0; j < fields.Length; j++)
            {
                if ((!fields[j].IsPrivate || fields[j].GetCustomAttributes(typeof(SerializeField), false).Length != 0) && TaskInspector.isFieldLinked(fields[j]))
                {
                    if (fields[j].FieldType.IsArray)
                    {
                        Task[] array = fields[j].GetValue(nodeDesigner.Task) as Task[];
                        if (array != null)
                        {
                            for (int k = array.Length - 1; k > -1; k--)
                            {
                                TaskInspector.referenceTasks(array[k], nodeDesigner.Task, fields[j], ref flag, ref flag2, false, false, false);
                            }
                        }
                    }
                    else
                    {
                        Task task = fields[j].GetValue(nodeDesigner.Task) as Task;
                        if (task != null)
                        {
                            TaskInspector.referenceTasks(task, nodeDesigner.Task, fields[j], ref flag, ref flag2, false, false, false);
                        }
                    }
                }
            }
            DetachedNodes.Remove(nodeDesigner);
            BehaviorUndo.DestroyObject(nodeDesigner.Task, false);
            BehaviorUndo.DestroyObject(nodeDesigner, false);
        }
        #endregion
    }
}
