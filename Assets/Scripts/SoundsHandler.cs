using UnityEngine;

public class SoundsHandler : MonoBehaviour
{
    [SerializeField]
    private AudioSource _source;
    [SerializeField]
    private AudioClip _mergeClip;
    [SerializeField]
    private AudioClip _newFruitClip;
    [SerializeField]
    private AudioClip _bombClip;
    [SerializeField]
    private AudioClip _collidedClip;
    [SerializeField]
    private AudioClip _bananasClip;


    
   
    // public void Initialize(FishInstantiator fishInstantiator, CollisionHandler collisionHandler, BubblesPull bubblesPull, FruitsUpgraderAbility fruitsUpgraderAbility)
    // {
    //     _fishInstantiator = fishInstantiator;
    //     _collisionHandler = collisionHandler;
    //     _bubblesPull = bubblesPull;
    //     _fruitsUpgraderAbility = fruitsUpgraderAbility;
    //     _fishInstantiator.OnFruitInstantiatedAtTop += PlayNewFish;
    //     _fishInstantiator.OnBombInstantiated += SubscribeOnBomb;
    //     _collisionHandler.OnCollisionDone += PlayMerge;
    //     _bubblesPull.OnBananasAppeared += PlayBubbles;
    //     _fruitsUpgraderAbility.OnFruitUpgraded += PlayMerge;
    // }
    //
    // private void Update()
    // {
    //     if (!GameInfo.IsSoundOn)
    //         _source.mute = true;
    //     else
    //         _source.mute = false;
    // }
    //
    // private void PlayNewFish()
    // {
    //     _source.PlayOneShot(_newFruitClip);
    // }
    //
    // private void PlayMerge()
    // {
    //     _source.PlayOneShot(_mergeClip);
    // }
    //
    // private void SubscribeOnBomb(Bomb bomb)
    // {
    //     _bomb = bomb;
    //     _bomb.OnBombExploded += PlayBomb;
    // }
    //
    // private void PlayBomb() 
    // {
    //     UnsubscribeBomb();
    //     _source.PlayOneShot(_bombClip);
    // }
    //
    // private void UnsubscribeBomb()
    // {
    //     _bomb.OnBombExploded -= PlayBomb;
    // }
    //
    // private void PlayCollided()
    // {
    //     _source.PlayOneShot(_collidedClip);
    // }
    //
    // private void PlayBubbles()
    // {
    //     _source.PlayOneShot(_bananasClip);
    // }
    //
    // private void OnDestroy()
    // {
    //     _fishInstantiator.OnFruitInstantiatedAtTop -= PlayNewFish;
    //     _fishInstantiator.OnBombInstantiated -= SubscribeOnBomb;
    //     _collisionHandler.OnCollisionDone -= PlayMerge;
    //     _bubblesPull.OnBananasAppeared -= PlayBubbles;
    //     _fruitsUpgraderAbility.OnFruitUpgraded -= PlayMerge;
    //
    // }
}
