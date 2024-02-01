using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCatchUI : MonoBehaviour
{
   public static PlayerCatchUI[] s_CatchUIs = new PlayerCatchUI[4];

   [SerializeField] private PlayerProfile _playerProfile;
   [SerializeField] private Image _catchImage;
   [SerializeField] private Material _eyeCloseMat;

   private Material _eyeCloseMatInst;
   private int _eyeCloseID;

   private delegate IEnumerator PlayerEyeCloseDel(float time);
   private PlayerEyeCloseDel _playerEyeCloseFunc;

   private void Awake()
   {
      Debug.Assert(_playerProfile != null, $"Player profile for PlayerCatchUI on {gameObject.name} is null!");
      Debug.Assert(_catchImage != null, $"Player catch image for PlayerCatchUI on {gameObject.name} is null!");
      _catchImage.enabled = false;
      _eyeCloseMatInst = new Material(_eyeCloseMat);
      Shader shader = _eyeCloseMatInst.shader;
      _eyeCloseID = shader.GetPropertyNameId(shader.FindPropertyIndex("_CloseTime"));
      _playerEyeCloseFunc = Anim;
      _catchImage.material = _eyeCloseMatInst;
   }

   private void Start()
   {
      s_CatchUIs[_playerProfile.GetPlayerID()] = this;
   }

   private void OnDestroy()
   {
      s_CatchUIs[_playerProfile.GetPlayerID()] = null;
   }

   public static void Catch(int ID, float time)
   {
      if (s_CatchUIs[ID] != null) 
      {
         s_CatchUIs[ID].StartAnim(time);
      }
      else
      {
         Debug.LogError("Catch UI was not created for this player!");
      }
   }

   private void StartAnim(float time)
   {
      _catchImage.enabled = true;
      StartCoroutine(_playerEyeCloseFunc(time));
   }

   private IEnumerator Anim(float time)
   {
      float t = 0;
      while (t <= time)
      {
         t += Time.deltaTime;
         _eyeCloseMatInst.SetFloat(_eyeCloseID, t / time);
         yield return null;
      }

      yield return new WaitForSeconds(0.1f);
      
      while (t >= 0)
      {
         t -= Time.deltaTime;
         _eyeCloseMatInst.SetFloat(_eyeCloseID, t / time);
         yield return null;
      }
      
      _eyeCloseMatInst.SetFloat(_eyeCloseID, 0);
      _catchImage.enabled = false;
   }
  
}
