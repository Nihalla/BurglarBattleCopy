// Joshua Weston

using System;
using System.Collections;
using UnityEngine;
using PlayerControllers;
using TMPro;

public class CoinDepositController : MonoBehaviour, IInteractable
{
    public int totalLoot;

    private SphereCollider _col;
    private Loot _loot;
    private TextMeshProUGUI _lootCounter;
    private Animator _canvasAnim;

    public GameObject depositModel;
    private MeshRenderer[] _meshRenderers = new MeshRenderer[1];

    public FirstPersonController.PlayerTeam teamDeposit;

    private delegate IEnumerator EmptyDel();
    private EmptyDel AnimResetFunc;

    private void Awake()
    {
        // NOTE(Zack): pre-allocating delegate function pointer
        AnimResetFunc = AnimReset;
    }

    private void Start()
    {
        _col = gameObject.AddComponent<SphereCollider>();
        _col.isTrigger = true;
        _col.radius = 2;

        _lootCounter = GetComponentInChildren<TextMeshProUGUI>();
        _lootCounter.text = totalLoot.ToString();

        _canvasAnim = transform.GetChild(1).GetComponent<Animator>();

        _meshRenderers[0] = depositModel.GetComponent<MeshRenderer>();
    }

    public Span<MeshRenderer> GetInteractionMeshRenderers()
    {
        return _meshRenderers.AsSpan();
    }

    public void OnInteract(PlayerInteraction playerInteraction)
    {
        _loot = playerInteraction.GetComponent<Loot>();

        if (playerInteraction.PlayerProfile.GetTeam() == teamDeposit)
        {
            if(teamDeposit == FirstPersonController.PlayerTeam.TEAM_ONE)
            {
                GoldTransferToEnd.team1Gold += _loot.currentLoot;
                totalLoot += _loot.currentLoot;
                _loot.currentLoot = 0;

                _lootCounter.text = totalLoot.ToString();

                _canvasAnim.SetBool("lootDeposit", true);
                StartCoroutine(AnimResetFunc());

                GlobalEvents.OnTeamGoldUpdate();
            }
            else if (teamDeposit == FirstPersonController.PlayerTeam.TEAM_TWO)
            {
                GoldTransferToEnd.team2Gold += _loot.currentLoot;
                totalLoot += _loot.currentLoot;
                _loot.currentLoot = 0;

                _lootCounter.text = totalLoot.ToString();

                _canvasAnim.SetBool("lootDeposit", true);
                StartCoroutine(AnimResetFunc());

                GlobalEvents.OnTeamGoldUpdate();
            }
        }
    }

    private IEnumerator AnimReset()
    {
        // NOTE(Zack): we're doing the wait for second loop ourselves
        // so that we don't get any hidden memory allocations from WaitForSeconds()
        float timer = float.Epsilon;
        while (timer < 0.6f)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        // yield return new WaitForSeconds(0.6f);
        _canvasAnim.SetBool("lootDeposit", false);
    }

    public FirstPersonController.PlayerTeam GetPlayerTeam()
    {
        return teamDeposit;
    }
}
