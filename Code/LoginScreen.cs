using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataModel.Common;
using TMPro;
using DataModel.Client;
using UniRx;
using DataModel.Common.Messages;
using UnityEngine.Localization.Components;

public class LoginScreen : MonoBehaviour
{

    public GameObject ConnectionIssueRoot;

    private GameObject root;
    private GameObject loginMenuBackGround;
    private Image sheepPicture;


    [SerializeField] private TMP_InputField userName;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private TMP_InputField ipAddress;
    private Button loginButton;
    private Button loginSendButton;
    private Button registerButton;
    private Button registerSendButton;
    private Button backButton;
    private Button confirmIPButton;

    private GameObject statusTextRoot;
    [SerializeField] private TMP_Text statusText;

    private Image loadingImage;
    private Image backgroundPicture;

    public LoadingScreen loadingScreen;
    public ClientConnection clientConnection;
    LocalizeStringEvent localizedString;
    bool hasLoaded = false;

    Sprite menuBackGround;
    Sprite menuBackGroundBlur;
    Sprite sheep;
    Sprite sheepBlur;
    List<System.IDisposable> disposables = new List<System.IDisposable>();


    bool connectionEstablished;
    Animator anim;


    public void Awake()
    {
        

        ConnectionIssueRoot.SetActive(false);
        root = transform.Find("LoginPanel").gameObject;
        loginMenuBackGround = root.transform.Find("LoginMenuCanvas").gameObject;

        GameObject background = root.transform.Find("Background").gameObject;
        sheepPicture = background.transform.Find("Sheep").GetComponent<Image>();
        backgroundPicture = background.transform.Find("BackgroundImage").GetComponent<Image>();

        GameObject loginMenuCanvas = root.transform.Find("LoginMenuCanvas").gameObject;
        loginButton = loginMenuCanvas.transform.Find("LoginButton").GetComponent<Button>();
        loginSendButton = loginMenuCanvas.transform.Find("LoginSendButton").GetComponent<Button>();
        registerSendButton = loginMenuCanvas.transform.Find("RegisterSendButton").GetComponent<Button>();
        registerButton = loginMenuCanvas.transform.Find("RegisterButton").GetComponent<Button>();
        backButton = loginMenuCanvas.transform.Find("BackButton").GetComponent<Button>();
        statusTextRoot = loginMenuCanvas.transform.Find("StatusTextBackGround").gameObject;
        loadingImage = loginMenuCanvas.transform.Find("LoadingImage").GetComponent<Image>();
        confirmIPButton = loginMenuCanvas.transform.Find("ConfirmIP").GetComponent<Button>();

        confirmIPButton.gameObject.SetActive(false);
        ipAddress.gameObject.SetActive(false);




        localizedString = statusText.GetComponent<LocalizeStringEvent>();
        localizedString.StringReference.TableReference = "UIText";
        password.contentType = TMP_InputField.ContentType.Password;
        connectionEstablished = false;
        userName.characterLimit = 30;
        password.characterLimit = 30;
        statusTextRoot.SetActive(false);
        loginButton.gameObject.SetActive(true);
        loginSendButton.gameObject.SetActive(false);
        registerButton.gameObject.SetActive(true);
        registerSendButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        loadingImage.gameObject.SetActive(false);

        userName.gameObject.SetActive(false);
        password.gameObject.SetActive(false);
        anim = loadingImage.GetComponent<Animator>();

        menuBackGround = Resources.Load<Sprite>("SpritesAndUI/Background");
        menuBackGroundBlur = Resources.Load<Sprite>("SpritesAndUI/BackgroundBlur");

        sheep = Resources.Load<Sprite>("SpritesAndUI/Sheep2");
        sheepBlur = Resources.Load<Sprite>("SpritesAndUI/SheepBlur");
        backgroundPicture.sprite = menuBackGround;

        TryConnection();

    }

    public void TryConnection()
    {
        Debug.Log(clientConnection.ClientInstance + " instance");
        if (clientConnection.ClientInstance != null)
        {
            
            ServerConnection();
            Debug.Log("have clientInstance");
            var login = HandleActionResponse(clientConnection.ClientInstance.InboundTraffic, () =>
            {
                if (!hasLoaded)
                {

                    PlayerU.playerName = userName.text;
                    loadingScreen.StartLoading();
                    root.SetActive(false);
                    hasLoaded = true;

                }
                else
                {


                }

            }
         , MessageContext.LOGIN, MessageState.SUCCESS);

            var loginFail = HandleActionResponse(clientConnection.ClientInstance.InboundTraffic, () =>
            {
                ReturnToFirstState();
                statusTextRoot.SetActive(true);
                localizedString.StringReference.TableEntryReference = "LoginFailMessage";
            }
            , MessageContext.LOGIN, MessageState.ERROR);

            var registerSuccess = HandleActionResponse(clientConnection.ClientInstance.InboundTraffic, () =>
            {
                ReturnToFirstState();
                statusTextRoot.SetActive(true);
                localizedString.StringReference.TableEntryReference = "RegisterSuccessMessage";
            }
            , MessageContext.REGISTER, MessageState.SUCCESS);

            var registerFail = HandleActionResponse(clientConnection.ClientInstance.InboundTraffic, () =>
            {
                ReturnToFirstState();
                statusTextRoot.SetActive(true);
                localizedString.StringReference.TableEntryReference = "RegisterFailMessage";
            }
            , MessageContext.REGISTER, MessageState.ERROR);

            var hasConnection = clientConnection.ClientInstance.ClientConnectionState.WithLatestFrom(clientConnection.ClientInstance.ReconnectionState, (v1, v2) => new { v1, v2 }).ObserveOnMainThread().Subscribe(v =>
            {
                connectionEstablished = v.v1;
                Debug.Log("Connection" + v);



                if (v.v1)
                {
                    confirmIPButton.gameObject.SetActive(false);
                    ipAddress.gameObject.SetActive(false);
                    //reconnection
                    if (v.v2)
                    {
                        ConnectionIssueRoot.SetActive(false);
                        Debug.Log("Reconnected successfully!");
                    }
                    //first connection
                    else
                    {
                        Debug.Log("First Connection");
                        ServerConnection();
                    }

                }
                else
                {
                    //lost connection
                    if (v.v2)
                    {
                        ConnectionIssueRoot.SetActive(true);
                        Debug.Log("Reconnecting....");
                    }
                    //no connection (never connected before either)
                    else
                    {
                        Debug.Log("No Connection");
                        NoConnection();
                    }

                }
            });

            disposables.Add(login);
            disposables.Add(loginFail);
            disposables.Add(registerFail);
            disposables.Add(registerSuccess);
            disposables.Add(hasConnection);
        }
        else
        {
            Debug.Log("No Client Instance");
            NoConnection();
        }

    }

    public void ServerConnection()
    {
        anim.SetBool("play", false);
        loadingImage.gameObject.SetActive(false);
        statusTextRoot.SetActive(false);
        confirmIPButton.gameObject.SetActive(false);
        ipAddress.gameObject.SetActive(false);
        EnableMainMenu();
    }
    public void NoConnection()
    {
        DisableMainMenu();
        statusTextRoot.SetActive(true);
        localizedString.StringReference.TableEntryReference = "EstablishingConnection";
        confirmIPButton.gameObject.SetActive(true);
        ipAddress.gameObject.SetActive(true);

        loadingImage.gameObject.SetActive(true);
        anim.SetBool("play", true);

    }
    public void EnableMainMenu()
    {
        ReturnToFirstState();
    }
    public void DisableMainMenu()
    {
        loadingImage.gameObject.SetActive(true);
        userName.gameObject.SetActive(false);
        password.gameObject.SetActive(false);
        statusTextRoot.SetActive(false);
        loginButton.gameObject.SetActive(false);
        loginSendButton.gameObject.SetActive(false);
        registerButton.gameObject.SetActive(false);
        registerSendButton.gameObject.SetActive(false);
        sheepPicture.sprite = sheep;
        backgroundPicture.sprite = menuBackGround;
        backButton.gameObject.SetActive(false);
        loadingImage.gameObject.SetActive(false);
    }

    public void ReturnToFirstState()
    {
        loadingImage.gameObject.SetActive(false);
        userName.gameObject.SetActive(false);
        password.gameObject.SetActive(false);
        statusTextRoot.SetActive(false);
        loginButton.gameObject.SetActive(true);
        loginSendButton.gameObject.SetActive(false);
        registerButton.gameObject.SetActive(true);
        registerSendButton.gameObject.SetActive(false);
        sheepPicture.sprite = sheep;
        backgroundPicture.sprite = menuBackGround;
        backButton.gameObject.SetActive(false);
        loadingImage.gameObject.SetActive(false);
    }


    public void Login()
    {
        loginButton.gameObject.SetActive(false);
        loginSendButton.gameObject.SetActive(true);
        registerButton.gameObject.SetActive(false);
        userName.gameObject.SetActive(true);
        password.gameObject.SetActive(true);
        sheepPicture.sprite = sheepBlur;
        backgroundPicture.sprite = menuBackGroundBlur;
        backButton.gameObject.SetActive(true);


    }

    System.IDisposable HandleActionResponse(System.IObservable<IMessage> responseStream, System.Action action, MessageContext context, MessageState state)
    {
        
        return responseStream.Where(v => v is UserActionMessage)
                             .Select(v => v as UserActionMessage)
                             .Where(v => v.MessageContext == context)
                             .Where(v => v.MessageState == state)
                             .ObserveOnMainThread()
                             .Subscribe(v =>
                             {
                                 action.Invoke();
                             });
    }

    public void LoginSendButton()
    {
        if (userName.text == "" || password.text == "")
        {
            statusTextRoot.SetActive(true);
            localizedString.StringReference.TableEntryReference = "UsernameAndPassword";
        }
        else
        {
            clientConnection.ClientInstance.SendMessage(new AccountMessage() { Context = MessageContext.LOGIN, Name = userName.text, Password = password.text });
            statusTextRoot.SetActive(false);
        }
    }

    public void Register()
    {
        loginButton.gameObject.SetActive(false);
        registerSendButton.gameObject.SetActive(true);
        registerButton.gameObject.SetActive(false);
        userName.gameObject.SetActive(true);
        password.gameObject.SetActive(true);
        sheepPicture.sprite = sheepBlur;
        backgroundPicture.sprite = menuBackGroundBlur;
        backButton.gameObject.SetActive(true);
        statusTextRoot.SetActive(false);
    }


    public void Back()
    {
        ReturnToFirstState();

    }

    
    public void SendRegister()
    {
        if (userName.text == "" || password.text == "")
        {
            statusTextRoot.SetActive(true);
            localizedString.StringReference.TableEntryReference = "UsernameAndPassword";
           
        }
        else
        {
            clientConnection.ClientInstance.SendMessage(new AccountMessage() { Context = MessageContext.REGISTER, Name = userName.text, Password = password.text });
        }
    }
    
    public void SetIP()
    {
        clientConnection.SetIPAndConnect(ipAddress.text);
        TryConnection();
        Debug.Log(ipAddress.text);
    }



}
