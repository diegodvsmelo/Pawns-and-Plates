using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public bool isGamePaused = false; // A variável mestra de pausa

    [Header("UI References")]
    public GameObject slotPrefab;
    public Transform slotsContainer;
    public Transform rosterContainer;
    
    public MinigameUI minigameUI; 
    
    [Header("UI Panels")]
    public GameObject mapPanel;
    public GameObject missionPanel;
    public TaskSpawner spawner;

    public TaskData missaoTeste;
    private List<Slot> missionSlots = new List<Slot>(); 
    private List<Slot> rosterSlots = new List<Slot>();
    public int squadSlotNumbers = 6;

    [Header("Mission Window UI")]
    public TextMeshProUGUI missionTitleText;
    public TextMeshProUGUI missionDescText;

    public List<EmployeeData> startingEmployees; 
    public GameObject employeeCardPrefab;
    private bool isMissionRunning = false;
    public ResourceManager resourceManager;
    public DayCycleManager dayCycleManager;
    
    void Start()
    {
        SetupRosterSlots();
        SpawnStartingCrew();
        ShowMap();
    }

    void Update()
    {
        // Se o jogo estiver pausado, paramos atualizações visuais que dependem do tempo
        if (isGamePaused) return;

        if (missaoTeste != null && !isMissionRunning)
        {
            List<EmployeeData> currentSquad = GetSquadFromSlots();
            
            if (currentSquad.Count > 0)
            {
                float chance = CalculateSquadChance(missaoTeste, currentSquad);
                minigameUI.SetZoneSize(chance);
            }
            else
            {
                minigameUI.SetZoneSize(0);
            }
        }
    }

    public void ShowMap()
    {
        mapPanel.SetActive(true);
        missionPanel.SetActive(false);
        // O Spawner agora checa o isGamePaused, então só precisamos garantir que ele está ativo
        spawner.StartSpawning();
    }

    public void OpenMissionWindow(TaskData task)
    {
        mapPanel.SetActive(false);
        missionPanel.SetActive(true);

        this.missaoTeste = task;

        if (missionTitleText != null) missionTitleText.text = task.taskName;
        if (missionDescText != null) missionDescText.text = task.description;
        
        GenerateSlotsForTask(task);
        
        minigameUI.ResetUI(); 
        minigameUI.SetZoneSize(0);
    }

    public void CloseMissionWindow()
    {
        missaoTeste = null;
        ReturnCrewToRoster();
        missionPanel.SetActive(false);
        ShowMap(); 
    }

    void GenerateSlotsForTask(TaskData task)
    {
        foreach (Transform child in slotsContainer)
        {
            Destroy(child.gameObject);
        }
        missionSlots.Clear();

        for (int i = 0; i < task.maxSlots; i++)
        {
            GameObject newSlotObj = Instantiate(slotPrefab, slotsContainer);
            Slot slotScript = newSlotObj.GetComponent<Slot>();
            if (slotScript != null)
            {
                slotScript.isRoster = false;
                missionSlots.Add(slotScript);
            }
        }
    }

    public void OnDispatchButtonPress()
    {
        if (isMissionRunning) return;

        List<EmployeeData> currentSquad = GetSquadFromSlots();
        
        if (currentSquad.Count > 0)
        {
            isMissionRunning = true;
            ConsumeSquadStamina();

            float chance = CalculateSquadChance(missaoTeste, currentSquad);
            minigameUI.SetZoneSize(chance);
            
            bool isCritical = chance >= 100f; 
            StartCoroutine(ProcessMissionResult(chance, isCritical));
        }
        else
        {
            Debug.LogWarning("Squad vazio!");
        }
    }

    IEnumerator ProcessMissionResult(float chancePercent, bool isCritical)
    {
        if (isCritical)
        {
            Debug.Log("CRÍTICO! 100% de chance! Finalização Imediata!");
            yield return new WaitForSeconds(0.5f); 
        }
        else
        {
            yield return new WaitForSeconds(3f);
        }

        float roll = Random.Range(0f, 100f);
        bool isSuccess = isCritical || (roll <= chancePercent);

        float zoneWidth = (minigameUI.totalWidth * chancePercent) / 100f;
        if (zoneWidth >= minigameUI.totalWidth) zoneWidth = minigameUI.totalWidth - 1;
        float stopX = isSuccess ? Random.Range(0f, zoneWidth) : Random.Range(zoneWidth, minigameUI.totalWidth);
        minigameUI.StopPointer(stopX);
        
        if (isSuccess)
        {
            Debug.Log("SUCESSO!");
            resourceManager.ModifyMoney(missaoTeste.moneyReward);
            resourceManager.ModifyReputation(missaoTeste.reputationReward);
            if (dayCycleManager != null)
            {
                dayCycleManager.AddDailyEarnings(missaoTeste.moneyReward);
            }

            int xpToGive = isCritical ? missaoTeste.xpOnCritical : missaoTeste.xpOnSuccess;
            GiveSquadExperience(xpToGive);
        }
        else
        {
            Debug.Log("FALHA!");
            resourceManager.ModifyReputation(-missaoTeste.reputationPenalty);
            GiveSquadExperience(missaoTeste.xpOnFailure);
        }

        yield return new WaitForSeconds(2f);
        ReturnCrewToRoster();
        ShowMap();
        
        missaoTeste = null;
        isMissionRunning = false;
    }

    void GiveSquadExperience(int amount)
    {
        foreach (Slot slot in missionSlots) 
        {
            if (slot.transform.childCount > 0)
            {
                EmployeeCard card = slot.transform.GetChild(0).GetComponent<EmployeeCard>();
                if (card != null)
                {
                    card.AddExperience(amount);
                }
            }
        }
    }

    void ReturnCrewToRoster()
    {
        foreach (Slot missionSlot in missionSlots)
        {
            if (missionSlot.transform.childCount > 0)
            {
                Transform card = missionSlot.transform.GetChild(0);
                Draggable draggable = card.GetComponent<Draggable>();
                Slot emptyRosterSlot = FindFirstEmptyRosterSlot();

                if (emptyRosterSlot != null)
                {
                    card.SetParent(emptyRosterSlot.transform);
                    card.localPosition = Vector3.zero; 

                    if (draggable != null)
                    {
                        draggable.originalParent = emptyRosterSlot.transform;
                    }
                }
                else
                {
                    Debug.LogError("ERRO: Não há slots vazios no Roster para devolver o funcionário!");
                }
            }
        }
    }

    List<EmployeeData> GetSquadFromSlots()
    {
        List<EmployeeData> squad = new List<EmployeeData>();
        foreach (Slot slot in missionSlots)
        {
            if (slot.transform.childCount > 0)
            {
                EmployeeCard card = slot.transform.GetChild(0).GetComponent<EmployeeCard>();
                if (card != null && card.data != null) squad.Add(card.data);
            }
        }
        return squad;
    }

    public float CalculateSquadChance(TaskData task, List<EmployeeData> squad)
    {
        float totalSquadScore = 0;
        float totalWeights = 0;
        foreach (var req in task.requirements) {
            float attrSum = 0;
            foreach (var mem in squad) {
                if (req.category == TaskCategory.Cooking) attrSum += mem.cookingSkill;
                else if (req.category == TaskCategory.Service) attrSum += mem.serviceSkill;
                else if (req.category == TaskCategory.Operational) attrSum += mem.operationalSkill;
                else if (req.category == TaskCategory.Agility) attrSum += mem.agility;
            }
            totalSquadScore += attrSum * req.weight;
            totalWeights += req.weight;
        }
        if (totalWeights == 0) return 0;
        float finalScore = totalSquadScore / totalWeights;
        return Mathf.Clamp((finalScore / task.difficultyPoints) * 100f, 0f, 100f);
    }

    Slot FindFirstEmptyRosterSlot()
    {
        foreach (Slot slot in rosterSlots)
        {
            if (slot.transform.childCount == 0) return slot;
        }
        return null;
    }

    void SetupRosterSlots()
    {
        rosterSlots.Clear();
        foreach (Transform child in rosterContainer)
        {
            Slot slot = child.GetComponent<Slot>();
            if (slot != null) rosterSlots.Add(slot);
        }

        if (rosterSlots.Count == 0)
        {
            for (int i = 0; i < squadSlotNumbers; i++)
            {
                GameObject newSlot = Instantiate(slotPrefab, rosterContainer);
                Slot slotScript = newSlot.GetComponent<Slot>();
                slotScript.isRoster = true;
                rosterSlots.Add(slotScript);
            }
        }
    }

    void SpawnStartingCrew()
    {
        for (int i = 0; i < startingEmployees.Count; i++)
        {
            if (i < rosterSlots.Count)
            {
                GameObject newCard = Instantiate(employeeCardPrefab, rosterSlots[i].transform);
                newCard.GetComponent<EmployeeCard>().Setup(startingEmployees[i]);
                newCard.GetComponent<Draggable>().originalParent = rosterSlots[i].transform;
            }
        }
    }

    void ConsumeSquadStamina()
    {
        int cost = missaoTeste.staminaCost;
        foreach (Slot slot in missionSlots)
        {
            if (slot.transform.childCount > 0)
            {
                EmployeeCard card = slot.transform.GetChild(0).GetComponent<EmployeeCard>();
                if (card != null) card.ConsumeStamina(cost);
            }
        }
    }
}