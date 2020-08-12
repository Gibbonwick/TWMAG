using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{

	public GameObject combatButtons;
	public GameObject playerPrefab;
	public GameObject playerPrefab2;
	public GameObject playerPrefab3;

	public GameObject enemyPrefab;
	public GameObject enemyPrefab2;
	public GameObject enemyPrefab3;

	public Transform playerBattleStation;
	public Transform enemyBattleStation;

	
	public Transform playerBattleStation2;
	public Transform enemyBattleStation2;

	public Transform playerBattleStation3;
	public Transform enemyBattleStation3;

	Unit playerUnit;
	Unit playerUnit2;
	Unit playerUnit3;

	Unit enemyUnit;
	
	Unit enemyUnit2;

	Unit enemyUnit3;

	public Text dialogueText;

	public BattleHUD playerHUD;
	public BattleHUD enemyHUD;

	public BattleState state;

	private List<Unit> order = new List<Unit>();

	private List<Unit> goodGuys = new List<Unit>();

	private List<Unit> badGuys = new List<Unit>();
	private int currentOrder = 0;

	private Unit currentUnit;


    // Start is called before the first frame update
    void Start()
    {
		state = BattleState.START;
		StartCoroutine(SetupBattle());
    }

	IEnumerator SetupBattle()
	{
		GameObject playerGO = Instantiate(playerPrefab, playerBattleStation);
		playerUnit = playerGO.GetComponent<Unit>();
		goodGuys.Add(playerUnit);

		GameObject playerGO2 = Instantiate(playerPrefab2, playerBattleStation2);
		playerUnit2 = playerGO2.GetComponent<Unit>();
		goodGuys.Add(playerUnit2);

		GameObject playerGO3 = Instantiate(playerPrefab3, playerBattleStation3);
		playerUnit3 = playerGO3.GetComponent<Unit>();
		goodGuys.Add(playerUnit3);

		GameObject enemyGO = Instantiate(enemyPrefab, enemyBattleStation);
		enemyUnit = enemyGO.GetComponent<Unit>();
		badGuys.Add(enemyUnit);

		GameObject enemyGO2 = Instantiate(enemyPrefab2, enemyBattleStation2);
		enemyUnit2 = enemyGO2.GetComponent<Unit>();
		badGuys.Add(enemyUnit2);

		GameObject enemyGO3 = Instantiate(enemyPrefab3, enemyBattleStation3);
		enemyUnit3 = enemyGO3.GetComponent<Unit>();
		badGuys.Add(enemyUnit3);

		//todo: speed will eventually supply our order - random for now
		order.Add(playerUnit2);
		order.Add(enemyUnit2);
		order.Add(enemyUnit);
		order.Add(playerUnit);
		order.Add(enemyUnit3);
		order.Add(playerUnit3);
		
		dialogueText.text = "A group of enemies approaches...";
		combatButtons.SetActive(false);
		playerHUD.SetHUD(playerUnit);
		enemyHUD.SetHUD(enemyUnit);

		yield return new WaitForSeconds(2f);
		SelectUnit();
	}

	Unit getNextUnit(){
		if(currentOrder >= order.Count){
			currentOrder = 0;
		}
		return order[currentOrder++];
	}

	bool IsGameOver(){
		if(goodGuys.Count == 0 || badGuys.Count == 0){
			return true;
		}
		return false;
	}

    void SelectUnit()
    {	
		if(IsGameOver()){
			dialogueText.text = "GAME OVER!";
			combatButtons.SetActive(false);
			return;
		}

        currentUnit = getNextUnit();
		dialogueText.text = currentUnit.unitName + "is attacking!";
        if (checkFaction(currentUnit) == "GoodGuy")
        {
			//Good guy selected! players choice!
			state = BattleState.PLAYERTURN;
			combatButtons.SetActive(true);
            playerHUD.SetHUD(currentUnit);			
			PlayerTurn();
        }
        else
        {
			state = BattleState.ENEMYTURN;
            enemyHUD.SetHUD(currentUnit);
			StartCoroutine(EnemyTurn());
        }

    }

	string checkFaction(Unit actor){
		if(goodGuys.Contains(actor)){
			return "GoodGuy";
		}
		return "BadGuy";
	}

    IEnumerator PlayerAttack()
	{
		Random rnd = new Random();
		var unitToAttack = badGuys[Random.Range(0, badGuys.Count)];
		enemyHUD.SetHUD(unitToAttack);

		bool isDead = unitToAttack.TakeDamage(currentUnit.damage);

		enemyHUD.SetHP(unitToAttack.currentHP);
		dialogueText.text = currentUnit.unitName + "'s attack on " + unitToAttack.unitName + " was successful!";

		yield return new WaitForSeconds(2f);

		if(isDead)
		{
			dialogueText.text = unitToAttack.unitName + " Dies!";
			yield return new WaitForSeconds(2f);
			order.Remove(unitToAttack);
			badGuys.Remove(unitToAttack);
		}
		SelectUnit();	
	}

	IEnumerator EnemyTurn()
	{
		Random rnd = new Random();
		var unitToAttack = goodGuys[Random.Range(0, goodGuys.Count)];
		playerHUD.SetHUD(unitToAttack);		
		dialogueText.text = currentUnit.unitName + " attacks " + unitToAttack.unitName + "!";

		yield return new WaitForSeconds(1f);

		bool isDead = unitToAttack.TakeDamage(currentUnit.damage);

		playerHUD.SetHP(unitToAttack.currentHP);

		yield return new WaitForSeconds(1f);

		if(isDead)
		{
			dialogueText.text = unitToAttack.unitName + " Dies!";
			yield return new WaitForSeconds(2f);
			order.Remove(unitToAttack);
			goodGuys.Remove(unitToAttack);
		} 
		SelectUnit();

	}

	void EndBattle()
	{
		if(state == BattleState.WON)
		{
			dialogueText.text = "You won the battle!";
		} else if (state == BattleState.LOST)
		{
			dialogueText.text = "You were defeated.";
		}
	}

	void PlayerTurn()
	{
		dialogueText.text = "Choose an action for " + currentUnit.unitName + ":";
	}

	IEnumerator PlayerHeal()
	{
		currentUnit.Heal(5);

		playerHUD.SetHP(currentUnit.currentHP);
		dialogueText.text = currentUnit.unitName + " Healed!";

		yield return new WaitForSeconds(2f);

		state = BattleState.ENEMYTURN;
		SelectUnit();
	}

	IEnumerator PlayerShout()
	{
		currentUnit.Shout(5);

		dialogueText.text = currentUnit.unitName + " feels stronger!";

		yield return new WaitForSeconds(2f);

		state = BattleState.ENEMYTURN;
		SelectUnit();
	}

	public void OnAttackButton()
	{
		if (state != BattleState.PLAYERTURN)
			return;

		StartCoroutine(PlayerAttack());
	}

	public void OnHealButton()
	{
		if (state != BattleState.PLAYERTURN)
			return;

		StartCoroutine(PlayerHeal());
	}

	public void OnShoutButton()
	{
		if (state != BattleState.PLAYERTURN)
			return;

		StartCoroutine(PlayerShout());
	}
}
