using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DeepSpace.MobileControlMessages;

namespace DeepSpace
{
	public class MobileControlManager : MonoBehaviour
	{
		#region singleton
		//Here is a private reference only this class can access
		private static MobileControlManager _instance;
		//This is the public reference that other classes will use
		public static MobileControlManager Instance
		{
			get
			{
				//If _instance hasn't been set yet, we grab it from the scene!
				//This will only happen the first time this reference is used.
				if (_instance == null)
					_instance = GameObject.FindObjectOfType<MobileControlManager>();
				if (_instance == null)
					Debug.LogWarning("MobileControlManager component accessed but not found in scene!");
				return _instance;
			}
		}
		#endregion

		public MobileControlTcpManager tcpManager;

		private List<MobileControlView> _viewList = new List<MobileControlView>();
		private List<MobileControlPassiveView> _passiveViewList = new List<MobileControlPassiveView>();

		public void RegisterView(MobileControlView view)
		{
			_viewList.Add(view);

			_viewList.Sort(delegate(MobileControlView v1, MobileControlView v2)
			{
				return v1.ViewID.CompareTo(v2.ViewID);
			});

			if (_viewList.Count == 1)
			{
				ActivateView(view);
			}

			// Update information on mobile device:
			tcpManager.AddMessage(CreateWindowListMessage());
		}

		public void UnregisterView(MobileControlView view)
		{
			if (view.IsActive)
			{
				ActivateNextView();
			}

			_viewList.Remove(view);

			if (_viewList.Count > 0)
			{
				// Update information on mobile device:
				tcpManager.AddMessage(CreateWindowListMessage());
			}
			else
			{
				Debug.LogWarning("MobileControlManager.UnregisterView(): Every view has been unregistered. No active view is left!");
			}
		}

		public void RegisterPassiveView(MobileControlPassiveView passiveView)
		{
			_passiveViewList.Add(passiveView);
		}

		public void UnregisterPassiveView(MobileControlPassiveView passiveView)
		{
			_passiveViewList.Remove(passiveView);
		}

		public MobileControlView GetViewById(int id)
		{
			MobileControlView result = _viewList.Find(delegate(MobileControlView view)
			{
				return view.ViewID == id;
			});

			return result;
		}

		private void Start()
		{
			// RegisterView might be called n times during Awake(). Set the first one active:
			if (_viewList.Count > 0)
			{
				_viewList[0].EnableView();
			}
		}

		public MobileControlView GetCurrentView()
		{
			return _viewList.Find(delegate(MobileControlView view)
			{
				return view.IsActive == true;
			});
		}

		protected virtual void LateUpdate()
		{
			if (_viewList.Count > 0)
			{
				if (GetCurrentView() == null)
				{
					_viewList[0].EnableView();
				}

				Message message = null;
				while ((message = tcpManager.GetNextReceivedMessage()) != null)
				{
					switch (message.Command)
					{
						case Command.GET_WINDOWS:
							{
								tcpManager.AddMessage(CreateWindowListMessage());

								//Debug.Log("Received GetWindows Message.");
							}
							break;
						case Command.GET_WINDOW_BY_NUMBER:
							{
								GetWindowByNumberMessage getWindowByNumMsg = new GetWindowByNumberMessage(message.Bytes);
								getWindowByNumMsg.UnpackReceivedMessage();
								int requestedWindowNumber = (int)getWindowByNumMsg.WindowNumber;

								//Message viewMsg = null;

								if (requestedWindowNumber >= 0 && requestedWindowNumber < _viewList.Count)
								{
									MobileControlView nextView = _viewList[requestedWindowNumber];

									if (nextView.IsActive == false) // No view is active, so set one active (will send a setViewMessage to the client.)
									{
										ActivateView(nextView);
									}
									else // A view is currently active but the client asks again about the correct view (e.g. after a reconnect).
									{
										tcpManager.SetViewMessage(tcpManager.CurrentViewMessage);
									}
								}
								else
								{
									Debug.LogError("The requested Window-Number (" + requestedWindowNumber + ") is out of range of the amount of currently available windows (" + _viewList.Count + ").");
								}
								//Debug.Log("Received GetWindowByNumber Message.");
							}
							break;
						case Command.GET_SCENE_MENU:
							{
								tcpManager.AddMessage(CreateSceneMenuMessage());
							}
							break;
						case Command.CHOSE_SCENE:
							// Set the MSC as current view and tell it about the requested scene:
							{
								// A Menu Entry has been chosen. We send this information directly back to the mobile device so that it gets highlighted in the menu.
								ChoseSceneMessage choseSceneMsg = new ChoseSceneMessage(message.Bytes);
								choseSceneMsg.UnpackReceivedMessage();
								tcpManager.AddMessage(choseSceneMsg);
							}
							break;
						case Command.USE_MICRO_STORY_CONTROL:
							{
								// This is a special command button in the middle of the top view. 
								// You have to send a view to the mobile control, else the mobile control only shows the loading symbol.
								ActivateView(_viewList[0]); // I just take the first one here, but you can chose another one if you want (e.g. some kind of main view).
							}
							break;
						case Command.UNKNOWN:
							Debug.LogError("Received Unknown Message Command.");
							break;
						default:
							// Process any other message that is directed to the current view:
							GetCurrentView().ProcessMessage(message);
							break;
					}

					// Tell all passive views about the currently processed Message:
					foreach (MobileControlPassiveView passiveView in _passiveViewList)
					{
						passiveView.GotMessage(message);
					}
				}
			}
		}

		public void ActivateView(MobileControlView nextView)
		{
			// Disable current view:
			MobileControlView curView = GetCurrentView();
			if (curView != null)
			{
				curView.DisableView();
			}

			// Enable the next one:
			nextView.EnableView();
		}

		public void ActivateNextView()
		{
			// Disable current view:
			MobileControlView curView = GetCurrentView();
			curView.DisableView();

			int curIndex = _viewList.IndexOf(curView);
			curIndex = (curIndex + 1) % _viewList.Count;

			// Enable the next one:
			MobileControlView nextView = _viewList[curIndex];
			nextView.EnableView();
		}

		public void ActivatePreviousView()
		{
			// Disable current view:
			MobileControlView curView = GetCurrentView();
			curView.DisableView();

			int curIndex = _viewList.IndexOf(curView);
			curIndex--;
			if (curIndex < 0)
			{
				curIndex += _viewList.Count;
			}

			// Enable the next one:
			MobileControlView previousView = _viewList[curIndex];
			previousView.EnableView();
		}

		private WindowListMessage CreateWindowListMessage()
		{
			// Access MobileControlViewManagers View-List and extract all view names for this request.
			List<string> listElements = new List<string>(_viewList.Count);
			foreach (MobileControlView view in _viewList)
			{
				listElements.Add(view.viewName);
			}
			int currentIndex = _viewList.IndexOf(GetCurrentView());

			//Debug.Log("Sending new WindowList with " + _viewList.Count + " Elements.");

			if (currentIndex != -1)
			{
				return new WindowListMessage(listElements, (uint)currentIndex);
			}
			else
			{
				Debug.LogError("Currently there is no view set active!");
				return null;
			}
		}

		protected virtual SceneMenuMessage CreateSceneMenuMessage()
		{
			// Create the message:
			SceneMenuMessage sceneMenuMessage = new SceneMenuMessage();

			// Just for demonstration: (Remove this or add your needed Menu here)
			// Create a category:
			SceneMenuMessage.Category category0 = new SceneMenuMessage.Category();
			category0.categoryID = 0;
			category0.categoryName = "Demo Category 1";
			// Create a Section (Choseable Menu Entry belonging to a category)
			SceneMenuMessage.Category.Section category0Section0 = new SceneMenuMessage.Category.Section();
			category0Section0.sectionID = 0;
			category0Section0.sectionName = "Demo Section 1";
			category0.sectionList.Add(category0Section0); // Add Section to Category. You can add as many sections as you want.
			// Create another Section
			SceneMenuMessage.Category.Section category0Section1 = new SceneMenuMessage.Category.Section();
			category0Section1.sectionID = 1;
			category0Section1.sectionName = "Demo Section 2";
			category0.sectionList.Add(category0Section1); // Add Section to Category

			sceneMenuMessage.categorySectionList.Add(category0); // Add Category to SceneMenu. You can add as many categories to the menu as you want.
					
			return sceneMenuMessage;
		}
	}
}