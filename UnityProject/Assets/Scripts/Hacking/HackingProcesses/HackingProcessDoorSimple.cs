﻿using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This handles opening a network tab internally, and hence, doesn't require a HasNetworkTab component. Don't add one!
/// </summary>
public class HackingProcessDoorSimple : HackingProcessBase
{
	public NetTabType NetTabType = NetTabType.HackingPanel;

	[SerializeField]
	[Tooltip("The name that comes up when you interact with the object.")]
	private string doorName = "airlock";

	[SerializeField]
	[Tooltip("SpriteRender which is toggled on when the door panel is exposed.")]
	private SpriteRenderer hackPanelOverlay = null;

	[SerializeField]
	private Sprite hackPanelSprite = null;

	private static int? seed = null;

	private List<HackingNode> inputNodes = new List<HackingNode>();
	private List<HackingNode> outputNodes = new List<HackingNode>();

	private DoorController controller;
	public DoorController Controller
	{
		get
		{
			if (!controller)
			{
				controller = GetComponent<DoorController>();
			}
			return controller;
		}
	}

	private InteractableDoor intDoor;
	public InteractableDoor IntDoor
	{
		get
		{
			if (!intDoor)
			{
				intDoor = GetComponent<InteractableDoor>();
			}
			return intDoor;
		}
	}

	private void Awake()
	{
		if (seed == null)
		{
			seed = Random.Range(1, 100000);
		}
	}

	public override bool WillInteract(HandApply interaction, NetworkSide side)
	{
		if (!DefaultWillInteract.Default(interaction, side)) return false;
		if (interaction.TargetObject != gameObject) return false;
		if (IntDoor == null || !IntDoor.allowInput) return false;

		if (Validations.HasItemTrait(interaction.HandObject, CommonTraits.Instance.Screwdriver))
		{
			return true;
		}

		return WiresExposed;
	}

	public override void ClientPredictInteraction(HandApply interaction)
	{
	}

	public override void ServerRollbackClient(HandApply interaction) { }

	public override void ServerPerformInteraction(HandApply interaction)
	{
		//Do specific things when the wires are exposed.
		if (WiresExposed)
		{
			if (interaction.HandObject == null && interaction.Performer != null)
			{
				TabUpdateMessage.Send(interaction.Performer, gameObject, NetTabType, TabAction.Open);
				IntDoor.StartInputCoolDown();
				return;
			}
		}

		if (Validations.HasItemTrait(interaction.HandObject, CommonTraits.Instance.Screwdriver))
		{
			if (Controller != null)
			{
				Chat.AddExamineMsgFromServer(interaction.Performer,
					"You " + (WiresExposed ? "close" : "open") + " the " + doorName + "'s maintenance panel");
				ServerTryTogglePanel();
			}

			IntDoor.StartInputCoolDown();
			return;
		}
	}

	public void Shuffle(List<HackingNodeInfo> list)
	{
		System.Random rng = new System.Random((int)seed);
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = rng.Next(n + 1);
			HackingNodeInfo value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

	public override void ServerGenerateNodesFromNodeInfo()
	{
		List<HackingNodeInfo> infList = nodeInfo.nodeInfoList.ToList();
		Shuffle(infList);
		foreach (HackingNodeInfo inf in infList)
		{
				HackingNode newNode = new HackingNode();
				newNode.IsInput = inf.IsInput;
				newNode.IsOutput = inf.IsOutput;
				newNode.IsDeviceNode = inf.IsDeviceNode;
				newNode.InternalIdentifier = inf.InternalIdentifier;
				newNode.HiddenLabel = inf.HiddenLabel;
				newNode.PublicLabel = inf.PublicLabel;

			if (inf.IsInput)
			{
				inputNodes.Add(newNode);
			}
			else
			{
				outputNodes.Add(newNode);
			}
		}

		hackNodes = inputNodes.Concat(outputNodes).ToList();
	}

	public override void ClientGenerateNodesFromNodeInfo()
	{
		List<HackingNodeInfo> infList = nodeInfo.nodeInfoList.ToList();
		foreach (HackingNodeInfo inf in infList)
		{
			HackingNode newNode = new HackingNode();
			newNode.IsInput = inf.IsInput;
			newNode.IsOutput = inf.IsOutput;
			newNode.IsDeviceNode = inf.IsDeviceNode;
			newNode.PublicLabel = inf.PublicLabel;

			if (inf.IsInput)
			{
				inputNodes.Add(newNode);
			}
			else
			{
				outputNodes.Add(newNode);
			}
		}
		hackNodes = inputNodes.Concat(outputNodes).ToList();
	}

	public override void ServerLinkHackingNodes()
	{
		Controller.LinkHackNodes();
	}

	public void ServerTryTogglePanel()
	{
		if (!Controller.isPerformingAction)
		{
			ToggleWiresExposed();
		}
	}

	protected override void OnWiresExposed()
	{
		if (hackPanelOverlay != null && hackPanelSprite != null)
		{
			hackPanelOverlay.sprite = hackPanelSprite;
		}
	}

	protected override void OnWiresHidden()
	{
		if (hackPanelOverlay != null)
		{
			hackPanelOverlay.sprite = null;
		}
	}

	public override void OnDespawnServer(DespawnInfo info)
	{
		NetworkTabManager.Instance.RemoveTab(gameObject, NetTabType);
	}

}
