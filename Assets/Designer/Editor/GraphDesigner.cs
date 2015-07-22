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
		private NodeDesigner mEntryTask;

		private NodeDesigner mRootNode;

		private List<NodeDesigner> mDetachedNodes = new List<NodeDesigner>();

		private List<NodeDesigner> mSelectedNodes = new List<NodeDesigner>();

		private NodeDesigner mHoverNode;

		private NodeConnection mActiveNodeConnection;

		private List<NodeConnection> mSelectedNodeConnections = new List<NodeConnection>();

		[SerializeField]
		private int mNextTaskID;

		private List<int> mNodeSelectedID = new List<int>();

		[SerializeField]
		private int[] mPrevNodeSelectedID;

		public NodeDesigner RootNode
		{
			get
			{
				return this.mRootNode;
			}
		}

		public List<NodeDesigner> DetachedNodes
		{
			get
			{
				return this.mDetachedNodes;
			}
		}

		public List<NodeDesigner> SelectedNodes
		{
			get
			{
				return this.mSelectedNodes;
			}
		}

		public NodeDesigner HoverNode
		{
			get
			{
				return this.mHoverNode;
			}
			set
			{
				this.mHoverNode = value;
			}
		}

		public NodeConnection ActiveNodeConnection
		{
			get
			{
				return this.mActiveNodeConnection;
			}
			set
			{
				this.mActiveNodeConnection = value;
			}
		}

		public List<NodeConnection> SelectedNodeConnections
		{
			get
			{
				return this.mSelectedNodeConnections;
			}
		}

		public void OnEnable()
		{
            base.hideFlags = HideFlags.HideAndDontSave;
		}

		public bool OnInspectorUpdate()
		{
			if (this.mEntryTask == null)
			{
				return false;
			}
			bool result = this.mEntryTask.OnInspectorUpdate();
			if (this.mRootNode != null && this.mRootNode.OnInspectorUpdate())
			{
				result = true;
			}
			for (int i = 0; i < this.mDetachedNodes.Count; i++)
			{
				if (this.mDetachedNodes[i].OnInspectorUpdate())
				{
					result = true;
				}
			}
			return result;
		}

		public NodeDesigner addNode(BehaviorSource behaviorSource, Type type, Vector2 position)
		{
			BehaviorUndo.RegisterUndo("Add Task", this, true, true);
			BehaviorUndo.RegisterUndo("Add Task", behaviorSource.Owner.GetObject(), false, true);
			Task task;
			if (this.mEntryTask == null)
			{
				task = (ScriptableObject.CreateInstance("EntryTask") as Task);
				this.mEntryTask = ScriptableObject.CreateInstance<NodeDesigner>();
				this.mEntryTask.loadNode(task, behaviorSource, new Vector2(position.x, position.y - 120f), ref this.mNextTaskID);
				this.mEntryTask.makeEntryDisplay();
			}
			task = (ScriptableObject.CreateInstance(type) as Task);
			if (task == null)
			{
				Debug.LogError(string.Format("Unable to create task of type {0}. Is the class name the same as the file name?", type));
				return null;
			}
			NodeDesigner nodeDesigner = ScriptableObject.CreateInstance<NodeDesigner>();
			nodeDesigner.loadNode(task, behaviorSource, position, ref this.mNextTaskID);
			if (this.mEntryTask.OutgoingNodeConnections.Count == 0)
			{
				this.mActiveNodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
				this.mActiveNodeConnection.loadConnection(this.mEntryTask, NodeConnectionType.Outgoing);
				this.connectNodes(behaviorSource, nodeDesigner);
			}
			else
			{
				this.mDetachedNodes.Add(nodeDesigner);
			}
			return nodeDesigner;
		}

		public NodeDesigner nodeAt(Vector2 point, Vector2 offset)
		{
			if (this.mEntryTask == null)
			{
				return null;
			}
			for (int i = 0; i < this.mSelectedNodes.Count; i++)
			{
				if (this.mSelectedNodes[i].contains(point, offset, false))
				{
					return this.mSelectedNodes[i];
				}
			}
			NodeDesigner result;
			for (int j = this.mDetachedNodes.Count - 1; j > -1; j--)
			{
				if (this.mDetachedNodes[j] != null && (result = this.nodeChildrenAt(this.mDetachedNodes[j], point, offset)) != null)
				{
					return result;
				}
			}
			if (this.mRootNode != null && (result = this.nodeChildrenAt(this.mRootNode, point, offset)) != null)
			{
				return result;
			}
			if (this.mEntryTask.contains(point, offset, true))
			{
				return this.mEntryTask;
			}
			return null;
		}

		public NodeDesigner nodeChildrenAt(NodeDesigner nodeDesigner, Vector2 point, Vector2 offset)
		{
			if (nodeDesigner.contains(point, offset, true))
			{
				return nodeDesigner;
			}
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
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
			}
			return null;
		}

		public List<NodeDesigner> nodesAt(Rect rect, Vector2 offset)
		{
			List<NodeDesigner> list = new List<NodeDesigner>();
			if (this.mRootNode != null)
			{
				this.nodesChildrenAt(this.mRootNode, rect, offset, ref list);
			}
			for (int i = 0; i < this.mDetachedNodes.Count; i++)
			{
				this.nodesChildrenAt(this.mDetachedNodes[i], rect, offset, ref list);
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
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
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
			}
		}

		public bool isSelected(NodeDesigner nodeDesigner)
		{
			return this.mSelectedNodes.Contains(nodeDesigner);
		}

		public void select(NodeDesigner nodeDesigner)
		{
			this.select(nodeDesigner, true);
		}

		public void select(NodeDesigner nodeDesigner, bool addHash)
		{
			if (this.mSelectedNodes.Count == 1)
			{
				this.indicateReferencedTasks(this.mSelectedNodes[0].Task, false);
			}
			this.mSelectedNodes.Add(nodeDesigner);
			if (addHash)
			{
				this.mNodeSelectedID.Add(nodeDesigner.Task.ID);
			}
			nodeDesigner.select();
			if (this.mSelectedNodes.Count == 1)
			{
				this.indicateReferencedTasks(this.mSelectedNodes[0].Task, true);
			}
		}

		public void deselect(NodeDesigner nodeDesigner)
		{
			this.mSelectedNodes.Remove(nodeDesigner);
			this.mNodeSelectedID.Remove(nodeDesigner.Task.ID);
			nodeDesigner.deselect();
			this.indicateReferencedTasks(nodeDesigner.Task, false);
		}

		public void deselectAllExcept(NodeDesigner nodeDesigner)
		{
			for (int i = this.mSelectedNodes.Count - 1; i >= 0; i--)
			{
				if (!this.mSelectedNodes[i].Equals(nodeDesigner))
				{
					this.mSelectedNodes.RemoveAt(i);
					this.mNodeSelectedID.RemoveAt(i);
					this.mSelectedNodes[i].deselect();
				}
			}
			this.indicateReferencedTasks(nodeDesigner.Task, false);
		}

		public void clearNodeSelection()
		{
			if (this.mSelectedNodes.Count == 1)
			{
				this.indicateReferencedTasks(this.mSelectedNodes[0].Task, false);
			}
			for (int i = 0; i < this.mSelectedNodes.Count; i++)
			{
				this.mSelectedNodes[i].deselect();
			}
			this.mSelectedNodes.Clear();
			this.mNodeSelectedID.Clear();
		}

		public void deselectWithParent(NodeDesigner nodeDesigner)
		{
			for (int i = this.mSelectedNodes.Count - 1; i >= 0; i--)
			{
				if (this.mSelectedNodes[i].hasParent(nodeDesigner))
				{
					this.deselect(this.mSelectedNodes[i]);
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

		public bool dragSelectedNodes(Vector2 delta, bool dragChildren, bool hasDragged)
		{
			if (this.mSelectedNodes.Count == 0)
			{
				return false;
			}
			bool flag = this.mSelectedNodes.Count == 1;
			for (int i = 0; i < this.mSelectedNodes.Count; i++)
			{
				this.dragTask(this.mSelectedNodes[i], delta, dragChildren, hasDragged);
			}
			if (flag && dragChildren && this.mSelectedNodes[0].IsEntryDisplay && this.mRootNode != null)
			{
				this.dragTask(this.mRootNode, delta, dragChildren, hasDragged);
			}
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
			if (nodeDesigner.IsParent && dragChildren)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
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

		public bool drawNodes(Vector2 mousePosition, Vector2 offset, float graphZoom)
		{
			if (this.mEntryTask == null)
			{
				return false;
			}
			this.mEntryTask.drawNodeConnection(offset, graphZoom, false);
			if (this.mRootNode != null)
			{
				this.drawNodeConnectionChildren(this.mRootNode, offset, graphZoom, this.mRootNode.Task.NodeData.Disabled);
			}
			for (int i = 0; i < this.mDetachedNodes.Count; i++)
			{
				this.drawNodeConnectionChildren(this.mDetachedNodes[i], offset, graphZoom, this.mDetachedNodes[i].Task.NodeData.Disabled);
			}
			for (int j = 0; j < this.mSelectedNodeConnections.Count; j++)
			{
				this.mSelectedNodeConnections[j].drawConnection(offset, graphZoom, this.mSelectedNodeConnections[j].OriginatingNodeDesigner.isDisabled());
			}
			if (mousePosition != new Vector2(-1f, -1f) && this.mActiveNodeConnection != null)
			{
				this.mActiveNodeConnection.HorizontalHeight = (this.mActiveNodeConnection.OriginatingNodeDesigner.getConnectionPosition(offset, this.mActiveNodeConnection.NodeConnectionType).y + mousePosition.y) / 2f;
				this.mActiveNodeConnection.drawConnection(this.mActiveNodeConnection.OriginatingNodeDesigner.getConnectionPosition(offset, this.mActiveNodeConnection.NodeConnectionType), mousePosition, graphZoom, this.mActiveNodeConnection.NodeConnectionType == NodeConnectionType.Outgoing && this.mActiveNodeConnection.OriginatingNodeDesigner.isDisabled());
			}
			this.mEntryTask.drawNode(offset, false, false);
			bool result = false;
			if (this.mRootNode != null && this.drawNodeChildren(this.mRootNode, offset, this.mRootNode.Task.NodeData.Disabled))
			{
				result = true;
			}
			for (int k = 0; k < this.mDetachedNodes.Count; k++)
			{
				if (this.drawNodeChildren(this.mDetachedNodes[k], offset, this.mDetachedNodes[k].Task.NodeData.Disabled))
				{
					result = true;
				}
			}
			for (int l = 0; l < this.mSelectedNodes.Count; l++)
			{
				if (this.mSelectedNodes[l].drawNode(offset, true, this.mSelectedNodes[l].isDisabled()))
				{
					result = true;
				}
			}
			if (this.mRootNode != null)
			{
				this.drawNodeCommentChildren(this.mRootNode, offset);
			}
			for (int m = 0; m < this.mDetachedNodes.Count; m++)
			{
				this.drawNodeCommentChildren(this.mDetachedNodes[m], offset);
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
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
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
			}
			return result;
		}

		private void drawNodeConnectionChildren(NodeDesigner nodeDesigner, Vector2 offset, float graphZoom, bool disabledNode)
		{
			if (nodeDesigner == null)
			{
				return;
			}
			if (!nodeDesigner.Task.NodeData.Collapsed)
			{
				nodeDesigner.drawNodeConnection(offset, graphZoom, nodeDesigner.Task.NodeData.Disabled || disabledNode);
				if (nodeDesigner.IsParent)
				{
					ParentTask parentTask = nodeDesigner.Task as ParentTask;
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
				}
			}
		}

		private void drawNodeCommentChildren(NodeDesigner nodeDesigner, Vector2 offset)
		{
			if (nodeDesigner == null)
			{
				return;
			}
			nodeDesigner.drawNodeComment(offset);
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
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
			}
		}

		public void removeNode(NodeDesigner nodeDesigner)
		{
			if (nodeDesigner.IsEntryDisplay)
			{
				return;
			}
			if (nodeDesigner.IsParent)
			{
				for (int i = 0; i < nodeDesigner.OutgoingNodeConnections.Count; i++)
				{
					NodeDesigner destinationNodeDesigner = nodeDesigner.OutgoingNodeConnections[i].DestinationNodeDesigner;
					BehaviorUndo.RegisterUndo("Delete", destinationNodeDesigner, false, true);
					this.mDetachedNodes.Add(destinationNodeDesigner);
					destinationNodeDesigner.ParentNodeDesigner = null;
				}
			}
			if (nodeDesigner.ParentNodeDesigner != null)
			{
				BehaviorUndo.RegisterUndo("Delete", nodeDesigner.ParentNodeDesigner, false, true);
				BehaviorUndo.RegisterCompleteUndo("Delete", nodeDesigner.ParentNodeDesigner.Task);
				nodeDesigner.ParentNodeDesigner.removeChildNode(nodeDesigner);
			}
			if (this.mRootNode != null && this.mRootNode.Equals(nodeDesigner))
			{
				this.mEntryTask.removeChildNode(nodeDesigner);
				this.mRootNode = null;
			}
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
			this.mDetachedNodes.Remove(nodeDesigner);
			BehaviorUndo.DestroyObject(nodeDesigner.Task, false);
			BehaviorUndo.DestroyObject(nodeDesigner, false);
		}

		public bool nodeCanOriginateConnection(NodeDesigner nodeDesigner, NodeConnection connection)
		{
			return !nodeDesigner.IsEntryDisplay || (nodeDesigner.IsEntryDisplay && connection.NodeConnectionType == NodeConnectionType.Outgoing);
		}

		public bool nodeCanAcceptConnection(NodeDesigner nodeDesigner, NodeConnection connection)
		{
			if ((!nodeDesigner.IsEntryDisplay || connection.NodeConnectionType != NodeConnectionType.Incoming) && (nodeDesigner.IsEntryDisplay || (!nodeDesigner.IsParent && (nodeDesigner.IsParent || connection.NodeConnectionType != NodeConnectionType.Outgoing))))
			{
				return false;
			}
			if (nodeDesigner.IsEntryDisplay || connection.OriginatingNodeDesigner.IsEntryDisplay)
			{
				return true;
			}
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
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
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
			}
			return false;
		}

		public void connectNodes(BehaviorSource behaviorSource, NodeDesigner nodeDesigner)
		{
			NodeConnection nodeConnection = this.mActiveNodeConnection;
			this.mActiveNodeConnection = null;
			if (nodeConnection != null && !nodeConnection.OriginatingNodeDesigner.Equals(nodeDesigner))
			{
				NodeDesigner originatingNodeDesigner = nodeConnection.OriginatingNodeDesigner;
				BehaviorUndo.RegisterUndo("Connection", this, true, true);
				BehaviorUndo.RegisterUndo("Connection", behaviorSource.Owner.GetObject(), false, true);
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
				if (nodeConnection.OriginatingNodeDesigner.IsEntryDisplay)
				{
					this.mRootNode = nodeConnection.DestinationNodeDesigner;
				}
				this.mDetachedNodes.Remove(nodeConnection.DestinationNodeDesigner);
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
			if (nodeDesigner.IsEntryDisplay)
			{
				if (nodeDesigner.OutgoingNodeConnections.Count == 1)
				{
					this.removeConnection(nodeDesigner.OutgoingNodeConnections[0], "Connect");
					return;
				}
			}
			else
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.Children != null && parentTask.Children.Count + 1 > parentTask.MaxChildren())
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
			}
		}

		public void nodeConnectionsAt(Vector2 point, Vector2 offset, ref List<NodeConnection> nodeConnections)
		{
			if (this.mEntryTask == null)
			{
				return;
			}
			this.nodeChildrenConnectionsAt(this.mEntryTask, point, offset, ref nodeConnections);
			if (this.mRootNode != null)
			{
				this.nodeChildrenConnectionsAt(this.mRootNode, point, offset, ref nodeConnections);
			}
			for (int i = 0; i < this.mDetachedNodes.Count; i++)
			{
				this.nodeChildrenConnectionsAt(this.mDetachedNodes[i], point, offset, ref nodeConnections);
			}
		}

		public void nodeChildrenConnectionsAt(NodeDesigner nodeDesigner, Vector2 point, Vector2 offset, ref List<NodeConnection> nodeConnections)
		{
			nodeDesigner.connectionContains(point, offset, ref nodeConnections);
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
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
			}
		}

		public void removeConnection(NodeConnection nodeConnection, string undoName)
		{
			BehaviorUndo.RegisterUndo(undoName, nodeConnection.OriginatingNodeDesigner, false, true);
			BehaviorUndo.RegisterUndo(undoName, nodeConnection.OriginatingNodeDesigner.Task, false, true);
			BehaviorUndo.RegisterUndo(undoName, nodeConnection.DestinationNodeDesigner, false, true);
			this.mDetachedNodes.Add(nodeConnection.DestinationNodeDesigner);
			nodeConnection.OriginatingNodeDesigner.removeChildNode(nodeConnection.DestinationNodeDesigner);
			if (nodeConnection.OriginatingNodeDesigner.IsEntryDisplay)
			{
				this.mRootNode = null;
			}
		}

		public void moveChildNode(NodeDesigner nodeDesigner, int index, bool decreaseIndex)
		{
			nodeDesigner.moveChildNode(index, decreaseIndex);
		}

		public bool isSelected(NodeConnection nodeConnection)
		{
			for (int i = 0; i < this.mSelectedNodeConnections.Count; i++)
			{
				if (this.mSelectedNodeConnections[i].Equals(nodeConnection))
				{
					return true;
				}
			}
			return false;
		}

		public void select(NodeConnection nodeConnection)
		{
			this.mSelectedNodeConnections.Add(nodeConnection);
			nodeConnection.select();
		}

		public void deselect(NodeConnection nodeConnection)
		{
			this.mSelectedNodeConnections.Remove(nodeConnection);
			nodeConnection.deselect();
		}

		public void clearConnectionSelection()
		{
			for (int i = 0; i < this.mSelectedNodeConnections.Count; i++)
			{
				this.mSelectedNodeConnections[i].deselect();
			}
			this.mSelectedNodeConnections.Clear();
		}

		public void graphDirty()
		{
			if (this.mEntryTask == null)
			{
				return;
			}
			this.mEntryTask.markDirty();
			if (this.mRootNode != null)
			{
				this.markTaskDirty(this.mRootNode);
			}
			for (int i = this.mDetachedNodes.Count - 1; i > -1; i--)
			{
				this.markTaskDirty(this.mDetachedNodes[i]);
			}
		}

		private void markTaskDirty(NodeDesigner nodeDesigner)
		{
			nodeDesigner.markDirty();
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
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
			}
		}

		public void selectAll()
		{
			for (int i = this.mSelectedNodes.Count - 1; i > -1; i--)
			{
				this.deselect(this.mSelectedNodes[i]);
			}
			if (this.mRootNode != null)
			{
				this.selectAll(this.mRootNode);
			}
			for (int j = this.mDetachedNodes.Count - 1; j > -1; j--)
			{
				this.selectAll(this.mDetachedNodes[j]);
			}
		}

		private void selectAll(NodeDesigner nodeDesigner)
		{
			this.select(nodeDesigner);
			if (nodeDesigner.Task.GetType().IsSubclassOf(typeof(ParentTask)))
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
				if (parentTask.Children != null)
				{
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						this.selectAll(parentTask.Children[i].NodeData.NodeDesigner as NodeDesigner);
					}
				}
			}
		}

		public List<TaskSerializer> copy()
		{
			List<TaskSerializer> list = new List<TaskSerializer>();
			for (int i = 0; i < this.mSelectedNodes.Count; i++)
			{
				TaskSerializer taskSerializer;
				if ((taskSerializer = TaskCopier.CopySerialized(this.mSelectedNodes[i].Task)) != null)
				{
					if (this.mSelectedNodes[i].IsParent)
					{
						ParentTask parentTask = this.mSelectedNodes[i].Task as ParentTask;
						if (parentTask.Children != null)
						{
							List<int> list2 = new List<int>();
							for (int j = 0; j < parentTask.Children.Count; j++)
							{
								int item;
								if ((item = this.mSelectedNodes.IndexOf(parentTask.Children[j].NodeData.NodeDesigner as NodeDesigner)) != -1)
								{
									list2.Add(item);
								}
							}
							taskSerializer.childrenIndex = list2;
						}
					}
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
						this.mDetachedNodes.Remove(list[taskSerializer2.childrenIndex[l]]);
					}
				}
			}
			this.save(behaviorSource);
			return true;
		}

		public bool delete(BehaviorSource behaviorSource)
		{
			BehaviorUndo.RegisterUndo("Delete", this, true, true);
			BehaviorUndo.RegisterUndo("Delete", behaviorSource.Owner.GetObject(), false, true);
			bool flag = false;
			if (this.mSelectedNodeConnections != null)
			{
				for (int i = 0; i < this.mSelectedNodeConnections.Count; i++)
				{
					this.removeConnection(this.mSelectedNodeConnections[i], "Delete");
				}
				this.mSelectedNodeConnections.Clear();
				flag = true;
			}
			if (this.mSelectedNodes != null)
			{
				for (int j = 0; j < this.mSelectedNodes.Count; j++)
				{
					this.removeNode(this.mSelectedNodes[j]);
				}
				this.mSelectedNodes.Clear();
				flag = true;
			}
			if (flag)
			{
				this.mNextTaskID = 0;
				this.mEntryTask.setID(ref this.mNextTaskID);
				if (this.mRootNode != null)
				{
					this.mRootNode.setID(ref this.mNextTaskID);
				}
				for (int k = 0; k < this.mDetachedNodes.Count; k++)
				{
					this.mDetachedNodes[k].setID(ref this.mNextTaskID);
				}
				this.save(behaviorSource);
			}
			return flag;
		}

		public void identifyNode(NodeDesigner nodeDesigner)
		{
			nodeDesigner.identifyNode();
		}

		public void save(BehaviorSource behaviorSource)
		{
			if (this.mEntryTask == null)
			{
				return;
			}
			List<Task> list = new List<Task>();
			for (int i = 0; i < this.mDetachedNodes.Count; i++)
			{
				list.Add(this.mDetachedNodes[i].Task);
			}
			behaviorSource.save(this.mEntryTask.Task, (this.mRootNode != null) ? this.mRootNode.Task : null, list);
		}

		public bool Load(BehaviorSource behaviorSource, bool loadPrevBehavior, Vector2 nodePosition)
		{
            behaviorSource.CheckForJSONSerialization(behaviorSource.Owner != null && (PrefabUtility.GetPrefabType(behaviorSource.Owner.GetObject()) == PrefabType.Prefab || PrefabUtility.GetPrefabType(behaviorSource.Owner.GetObject()) == PrefabType.PrefabInstance));
			if (behaviorSource.EntryTask == null && behaviorSource.RootTask == null && behaviorSource.DetachedTasks == null)
			{
				this.clear(false);
				return false;
			}
			if (loadPrevBehavior)
			{
				this.mSelectedNodes.Clear();
				this.mSelectedNodeConnections.Clear();
				if (this.mPrevNodeSelectedID != null)
				{
					for (int i = 0; i < this.mPrevNodeSelectedID.Length; i++)
					{
						this.mNodeSelectedID.Add(this.mPrevNodeSelectedID[i]);
					}
					this.mPrevNodeSelectedID = null;
				}
			}
			else
			{
				this.clear(false);
			}
			this.mNextTaskID = 0;
			this.mEntryTask = null;
			this.mRootNode = null;
			this.mDetachedNodes.Clear();
			Task task;
			Task task2;
			List<Task> list;
			behaviorSource.load(out task, out task2, out list);
			int num = BehaviorDesignerUtility.JSONTaskCount(behaviorSource.Serialization);
			if (num > 0)
			{
				int num2 = BehaviorDesignerUtility.TaskCount(behaviorSource);
				if (num2 != num)
				{
					behaviorSource.CheckForJSONSerialization(true);
					behaviorSource.load(out task, out task2, out list);
				}
			}
			if (task == null)
			{
				if (task2 != null || (list != null && list.Count > 0))
				{
					task = (behaviorSource.EntryTask = (ScriptableObject.CreateInstance("EntryTask") as Task));
					this.mEntryTask = ScriptableObject.CreateInstance<NodeDesigner>();
					if (task2 != null)
					{
						this.mEntryTask.loadNode(task, behaviorSource, new Vector2(task2.NodeData.Position.x, task2.NodeData.Position.y - 120f), ref this.mNextTaskID);
					}
					else
					{
						this.mEntryTask.loadNode(task, behaviorSource, new Vector2(nodePosition.x, nodePosition.y - 120f), ref this.mNextTaskID);
					}
					this.mEntryTask.makeEntryDisplay();
					EditorUtility.SetDirty(behaviorSource.Owner.GetObject());
				}
			}
			else
			{
				this.mEntryTask = ScriptableObject.CreateInstance<NodeDesigner>();
				this.mEntryTask.loadTask(task, ref this.mNextTaskID);
				this.mEntryTask.makeEntryDisplay();
			}
			if (task2 != null)
			{
				this.mRootNode = ScriptableObject.CreateInstance<NodeDesigner>();
				this.mRootNode.loadTask(task2, ref this.mNextTaskID);
				NodeConnection nodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
				nodeConnection.loadConnection(this.mEntryTask, NodeConnectionType.Fixed);
				this.mEntryTask.addChildNode(this.mRootNode, nodeConnection, false);
				this.loadNodeSelection(this.mRootNode);
				if (this.mEntryTask.OutgoingNodeConnections.Count == 0)
				{
					this.mActiveNodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
					this.mActiveNodeConnection.loadConnection(this.mEntryTask, NodeConnectionType.Outgoing);
					this.connectNodes(behaviorSource, this.mRootNode);
				}
			}
			if (list != null)
			{
				for (int j = 0; j < list.Count; j++)
				{
					NodeDesigner nodeDesigner = ScriptableObject.CreateInstance<NodeDesigner>();
					nodeDesigner.loadTask(list[j], ref this.mNextTaskID);
					this.mDetachedNodes.Add(nodeDesigner);
					this.loadNodeSelection(nodeDesigner);
				}
			}
			return true;
		}

		public bool hasEntryNode()
		{
			return this.mEntryTask != null;
		}

		public Vector2 entryNodePosition()
		{
			return this.mEntryTask.Task.NodeData.Position;
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
			if (nodeDesigner.IsParent)
			{
				ParentTask parentTask = nodeDesigner.Task as ParentTask;
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
			}
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
			this.mSelectedNodes.Clear();
			this.mSelectedNodeConnections.Clear();
			this.mEntryTask = null;
			this.mRootNode = null;
			this.mDetachedNodes = new List<NodeDesigner>();
		}
	}
}
