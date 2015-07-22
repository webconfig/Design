using System;
using System.Collections;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
	public abstract class Task : ScriptableObject
	{
		protected Animation animation;

		protected AudioSource audio;

		protected Camera camera;

		protected Collider collider;

		protected Collider2D collider2D;

		protected ConstantForce constantForce;

		protected GameObject gameObject;

		protected GUIText guiText;

		protected GUITexture guiTexture;

		protected HingeJoint hingeJoint;

		protected Light light;

		protected NetworkView networkView;

		protected ParticleEmitter particleEmitter;

		protected ParticleSystem particleSystem;

		protected Renderer renderer;

		protected Rigidbody rigidbody;

		protected Rigidbody2D rigidbody2D;

		protected Transform transform;

		[SerializeField]
		private NodeData nodeData;

		[SerializeField]
		private Behavior owner;

		[SerializeField]
		private int id = -1;

		[SerializeField]
		private bool instant = true;

		private int referenceID = -1;

		public Animation Animation
		{
			set
			{
				this.animation = value;
			}
		}

		public AudioSource Audio
		{
			set
			{
				this.audio = value;
			}
		}

		public Camera Camera
		{
			set
			{
				this.camera = value;
			}
		}

		public Collider Collider
		{
			set
			{
				this.collider = value;
			}
		}

		public Collider2D Collider2D
		{
			set
			{
				this.collider2D = value;
			}
		}

		public ConstantForce ConstantForce
		{
			set
			{
				this.constantForce = value;
			}
		}

		public GameObject GameObject
		{
			set
			{
				this.gameObject = value;
			}
		}

		public GUIText GUIText
		{
			set
			{
				this.guiText = value;
			}
		}

		public GUITexture GUITexture
		{
			set
			{
				this.guiTexture = value;
			}
		}

		public HingeJoint HingeJoint
		{
			set
			{
				this.hingeJoint = value;
			}
		}

		public Light Light
		{
			set
			{
				this.light = value;
			}
		}

		public NetworkView NetworkView
		{
			set
			{
				this.networkView = value;
			}
		}

		public ParticleEmitter ParticleEmitter
		{
			set
			{
				this.particleEmitter = value;
			}
		}

		public ParticleSystem ParticleSystem
		{
			set
			{
				this.particleSystem = value;
			}
		}

		public Renderer Renderer
		{
			set
			{
				this.renderer = value;
			}
		}

		public Rigidbody Rigidbody
		{
			set
			{
				this.rigidbody = value;
			}
		}

		public Rigidbody2D Rigidbody2D
		{
			set
			{
				this.rigidbody2D = value;
			}
		}

		public Transform Transform
		{
			set
			{
				this.transform = value;
			}
		}

		public NodeData NodeData
		{
			get
			{
				return this.nodeData;
			}
			set
			{
				this.nodeData = value;
			}
		}

		public Behavior Owner
		{
			get
			{
				return this.owner;
			}
			set
			{
				this.owner = value;
			}
		}

		public int ID
		{
			get
			{
				return this.id;
			}
			set
			{
				this.id = value;
			}
		}

		public bool IsInstant
		{
			get
			{
				return this.instant;
			}
			set
			{
				this.instant = value;
			}
		}

		public int ReferenceID
		{
			get
			{
				return this.referenceID;
			}
			set
			{
				this.referenceID = value;
			}
		}

		public virtual void OnAwake()
		{
		}

		public virtual void OnStart()
		{
		}

		public virtual TaskStatus OnUpdate()
		{
			return TaskStatus.Success;
		}

		public virtual void OnEnd()
		{
		}

		public virtual void OnPause(bool paused)
		{
		}

		public virtual float GetPriority()
		{
			return 0f;
		}

		public virtual void OnBehaviorRestart()
		{
		}

		public virtual void OnReset()
		{
		}

		public virtual void OnSceneGUI()
		{
		}

		protected void StartCoroutine(string methodName)
		{
			this.Owner.StartTaskCoroutine(this, methodName);
		}

		protected void StartCoroutine(IEnumerator routine)
		{
			this.Owner.StartCoroutine(routine);
		}

		protected void StartCoroutine(string methodName, object value)
		{
			this.Owner.StartTaskCoroutine(this, methodName, value);
		}

		protected void StopCoroutine(string methodName)
		{
			this.Owner.StopTaskCoroutine(methodName);
		}

		protected void StopAllCoroutines()
		{
			this.Owner.StopAllTaskCoroutines();
		}

		public virtual void OnCollisionEnter(Collision collision)
		{
		}

		public virtual void OnCollisionExit(Collision collision)
		{
		}

		public virtual void OnCollisionStay(Collision collision)
		{
		}

		public virtual void OnTriggerEnter(Collider other)
		{
		}

		public virtual void OnTriggerExit(Collider other)
		{
		}

		public virtual void OnTriggerStay(Collider other)
		{
		}
	}
}
