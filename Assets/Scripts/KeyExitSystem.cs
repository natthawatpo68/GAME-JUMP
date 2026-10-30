using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class KeyExitManager : MonoBehaviour
{
    public int keysRequired = 3;
    public int keysCollected;
    public GameObject exitGate;
    public Renderer statusLight;
    private bool opening;
    private Text countText;
    private bool pickupLocked;

    private void Start() { CreateHud(); RefreshHud(); }

    public void Collect(KeyPickup pickup)
    {
        if (pickupLocked || pickup == null || pickup.collected) return;
        pickupLocked = true;
        pickup.HideOnlyThisKey(); keysCollected++;
        RefreshHud();
        StartCoroutine(UnlockAfterERelease());
        if (statusLight != null) statusLight.material.color = keysCollected >= keysRequired ? new Color(0.2f, 1f, 0.35f) : new Color(1f, 0.65f, 0.08f);
        if (keysCollected >= keysRequired && !opening) StartCoroutine(OpenExit());
    }

    private IEnumerator UnlockAfterERelease() { while (Keyboard.current != null && Keyboard.current.eKey.isPressed) yield return null; yield return null; pickupLocked = false; }

    private void RefreshHud() { if (countText != null) countText.text = keysCollected + "/" + keysRequired; }

    private void CreateHud()
    {
        var canvasObject = new GameObject("Key Counter HUD");
        var canvas = canvasObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 20;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();
        var panel = new GameObject("Key Counter Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasObject.transform, false);
        var pr = panel.GetComponent<RectTransform>(); pr.anchorMin = new Vector2(1,1); pr.anchorMax = new Vector2(1,1); pr.pivot = new Vector2(1,1); pr.anchoredPosition = new Vector2(-28,-28); pr.sizeDelta = new Vector2(170,76);
        panel.GetComponent<Image>().color = new Color(.03f,.06f,.13f,.88f);
        var icon = new GameObject("Key Icon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(panel.transform, false); var ir=icon.GetComponent<RectTransform>();ir.anchorMin=new Vector2(0,.5f);ir.anchorMax=new Vector2(0,.5f);ir.pivot=new Vector2(0,.5f);ir.anchoredPosition=new Vector2(15,0);ir.sizeDelta=new Vector2(50,50);icon.GetComponent<Image>().sprite=KeySprite();icon.GetComponent<Image>().preserveAspect=true;
        var label = new GameObject("Count", typeof(RectTransform), typeof(Text));
        label.transform.SetParent(panel.transform,false);var lr=label.GetComponent<RectTransform>();lr.anchorMin=new Vector2(1,.5f);lr.anchorMax=new Vector2(1,.5f);lr.pivot=new Vector2(1,.5f);lr.anchoredPosition=new Vector2(-14,0);lr.sizeDelta=new Vector2(92,55);countText=label.GetComponent<Text>();countText.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");countText.fontSize=34;countText.alignment=TextAnchor.MiddleRight;countText.color=Color.white;
    }

    private Sprite KeySprite()
    {
        const int n=64; var tex=new Texture2D(n,n,TextureFormat.RGBA32,false);var clear=new Color(0,0,0,0);var gold=new Color(1f,.72f,.08f,1f);
        for(int y=0;y<n;y++)for(int x=0;x<n;x++){bool ring=(x-19)*(x-19)+(y-40)*(y-40)<225 && (x-19)*(x-19)+(y-40)*(y-40)>81;bool stem=x>25&&x<55&&y>34&&y<45;bool tooth=(x>43&&x<49&&y>26&&y<41)||(x>51&&x<57&&y>30&&y<41);tex.SetPixel(x,y,(ring||stem||tooth)?gold:clear);}tex.Apply();return Sprite.Create(tex,new Rect(0,0,n,n),new Vector2(.5f,.5f));
    }

    private IEnumerator OpenExit()
    {
        opening=true; if(exitGate!=null){var blocker=exitGate.GetComponent<Collider>();if(blocker!=null)blocker.enabled=false;var start=exitGate.transform.position;var end=start+Vector3.up*6f;for(float t=0;t<1f;t+=Time.deltaTime/1.25f){exitGate.transform.position=Vector3.Lerp(start,end,t);yield return null;}exitGate.transform.position=end;}
    }
}
public class KeyPickup : MonoBehaviour
{
    public KeyExitManager manager;
    public int keyNumber;
    public GameObject pickupPrompt;
    [HideInInspector] public bool collected;
    private Transform nearbyPlayer;
    private const float PickupRange = 2.1f;
    public void HideOnlyThisKey() { collected = true; if (pickupPrompt != null) pickupPrompt.SetActive(false); var trigger = GetComponent<Collider>(); if (trigger != null) trigger.enabled = false; foreach (var renderer in GetComponentsInChildren<Renderer>(true)) renderer.enabled = false; gameObject.SetActive(false); }
    private void Update(){ transform.Rotate(0,100f*Time.deltaTime,0,Space.World); bool close = nearbyPlayer != null && (nearbyPlayer.position-transform.position).sqrMagnitude <= PickupRange*PickupRange; if (pickupPrompt != null && pickupPrompt.activeSelf != close) pickupPrompt.SetActive(close); if(close && Keyboard.current!=null && Keyboard.current.eKey.wasPressedThisFrame && manager!=null) manager.Collect(this); }
    private void OnTriggerEnter(Collider other){ var controller=other.GetComponentInParent<CharacterController>(); if(controller!=null) nearbyPlayer=controller.transform; }
    private void OnTriggerExit(Collider other){ var controller=other.GetComponentInParent<CharacterController>(); if(controller!=null && controller.transform==nearbyPlayer) nearbyPlayer=null; }
}