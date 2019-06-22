﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookshelfViewer : MonoBehaviour
{
	uint ListTop = 0;
	uint ListBottom = 2;

	public HashSet<ulong> WaitingOn = new HashSet<ulong>();
	public bool IsUnInitialised = true;
	public Dictionary<ulong, uint> IDToLocation = new Dictionary<ulong, uint>();

	public GameObject DynamicPanel;

	public SingleBookshelf UISingleBookshelf;

	public List<SingleBookshelf> BookshelfList = new List<SingleBookshelf>();

	private VariableViewerNetworking.NetFriendlyBookShelf _BookShelfIn;
	public VariableViewerNetworking.NetFriendlyBookShelf BookShelfIn
	{
		get
		{
			return _BookShelfIn;

		}
		set
		{

			_BookShelfIn = value;
			BookShelfInSetUp();
			return;
		}
	}

	private VariableViewerNetworking.NetFriendlyBookShelfView _BookShelfView;
	public VariableViewerNetworking.NetFriendlyBookShelfView BookShelfView
	{
		get
		{
			return _BookShelfView;

		}
		set
		{
			_BookShelfView = value;
			ValueSetUp();
			return;
		}
	}
	public void BookShelfInSetUp()
	{
		if (WaitingOn.Contains(_BookShelfIn.ID)) {
			WaitingOn.Remove(_BookShelfIn.ID);
			BookshelfList[(int)IDToLocation[_BookShelfIn.ID]].BookShelfView = _BookShelfIn;
		} 
	}

	public void ValueSetUp()
	{
		ListTop = 0;
		if (IsUnInitialised)
		{
			IsUnInitialised = false;
			for (uint i = 0; i < 3; i++)
			{
				Logger.Log("i" + i);
				SingleBookshelf SingleBookEntry = Instantiate(UISingleBookshelf) as SingleBookshelf;
				SingleBookEntry.transform.SetParent(DynamicPanel.transform);
				SingleBookEntry.transform.localScale = Vector3.one;
				WaitingOn.Add(_BookShelfView.HeldShelfIDs[i].ID);
				BookshelfList.Add(SingleBookEntry);
				IDToLocation[_BookShelfView.HeldShelfIDs[i].ID] = i;
				RequestBookshelfNetMessage.Send(_BookShelfView.HeldShelfIDs[i].ID);
			}
		}
		else { 
			for (uint i = 0; i < 3; i++)
			{
				Logger.Log(" _BookShelfView.HeldShelfIDs.Length > " + _BookShelfView.HeldShelfIDs.Length);
				if (!(_BookShelfView.HeldShelfIDs.Length <= (i)))
				{
					BookshelfList[(int)i].gameObject.SetActive(true);
					WaitingOn.Add(_BookShelfView.HeldShelfIDs[i].ID);
					IDToLocation[_BookShelfView.HeldShelfIDs[i].ID] = i;
					RequestBookshelfNetMessage.Send(_BookShelfView.HeldShelfIDs[i].ID);
					ListBottom = i;
				}
				else { 
					BookshelfList[(int)i].gameObject.SetActive(false);
				}
			}
		}
	}

	public void PageUp() {
		Logger.Log("Start of" + "ListTop > " + ListTop + " ListBottom > " + ListBottom);
		if (ListTop != 0) {			BookshelfList[2].BookShelfView = BookshelfList[1].BookShelfView;
			BookshelfList[1].BookShelfView = BookshelfList[0].BookShelfView;
			ListTop--;
			ListBottom--;
			Logger.Log("end of" + "ListTop > " + ListTop + " ListBottom > " + ListBottom);
			WaitingOn.Add(_BookShelfView.HeldShelfIDs[(int)ListTop].ID);
			IDToLocation[_BookShelfView.HeldShelfIDs[ListTop].ID] = 0;
			RequestBookshelfNetMessage.Send(_BookShelfView.HeldShelfIDs[ListTop].ID);
			//Request (_BookShelfView.HeldShelfIDs[ListTop].ID)
		}
	
	}
	public void PageDown() {
		Logger.Log("Start of" + "ListTop > " + ListTop + " ListBottom > " + ListBottom);
		if (!(_BookShelfView.HeldShelfIDs.Length <= (ListBottom+1)))
		{
			BookshelfList[0].BookShelfView = BookshelfList[1].BookShelfView;
			BookshelfList[1].BookShelfView = BookshelfList[2].BookShelfView;
			ListTop++;
			ListBottom++;
			Logger.Log("end of" + "ListTop > " + ListTop + " ListBottom > " + ListBottom);
			WaitingOn.Add(_BookShelfView.HeldShelfIDs[(int)ListBottom].ID);
			IDToLocation[_BookShelfView.HeldShelfIDs[ListBottom].ID] = 2;
			RequestBookshelfNetMessage.Send(_BookShelfView.HeldShelfIDs[ListBottom].ID);
		}
	}

	public void Start()
	{
		UIManager.Instance.BookshelfViewer = this;

	}

	public void GoToObscuringBookshelf()
	{
		Logger.LogError(_BookShelfView.ID.ToString());
		RequestBookshelfNetMessage.Send(_BookShelfView.ID, true);
	}
}
