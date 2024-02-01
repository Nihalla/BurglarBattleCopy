// Author: Zack Collins

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

// TODO(Zack): this is a temporary implementation just so that we're able to transfer to the EndScene
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(LayerMaskTrigger))]
public class VaultPickup : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private CoinDepositController _team1CoinDeposit;
    [SerializeField] private CoinDepositController _team2CoinDeposit;

    [Header("Scene Transition Settings")]
    [SceneAttribute, SerializeField] private int _endScene;

    private BoxCollider _boxCollider;
    private LayerMaskTrigger _layerTrigger;

    private void Awake()
    {
        Debug.Assert(_team1CoinDeposit != null, "_team1CoinDeposit is null. Please set in the inspector", this);
        Debug.Assert(_team2CoinDeposit != null, "_team2CoinDeposit is null. Please set in the inspector", this);

        _boxCollider = GetComponent<BoxCollider>();
        _boxCollider.isTrigger = true;

        _layerTrigger = GetComponent<LayerMaskTrigger>();
    }

    private void OnTriggerEnter(Collider other)
    {
        GoldTransferToEnd.team1Gold = _team1CoinDeposit.totalLoot;
        GoldTransferToEnd.team2Gold = _team2CoinDeposit.totalLoot;
        TransferToEndScene();
    }

    private async void TransferToEndScene()
    {
        FadeTransition.instance.FadeIn(); // Fade to the loading screen
        await Task.Delay(1000);

        SceneManager.LoadSceneAsync(_endScene);
    }
}
