using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGamePaused = false;

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
    private TaskPin selectedTaskPin;

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

    // OBSERVERS
    public event Action<TaskData> OnMissionWindowOpened;
    public event Action<TaskData> OnMissionWindowClosed;
    public event Action<TaskData> OnMissionStarted;
    public event Action<TaskData, bool, bool> OnMissionFinished;
    public event Action<List<EmployeeData>> OnSquadChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (resourceManager == null)
            resourceManager = ResourceManager.Instance;

        if (dayCycleManager == null)
            dayCycleManager = DayCycleManager.Instance;

        if (spawner == null)
            spawner = TaskSpawner.Instance;

        SetupRosterSlots();
        SpawnStartingCrew();
        ShowMap();
    }

    private void Update()
    {
        if (isGamePaused)
            return;

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
        if (mapPanel != null)
            mapPanel.SetActive(true);

        if (missionPanel != null)
            missionPanel.SetActive(false);

        if (spawner != null)
            spawner.StartSpawning();
    }

    public void OpenMissionWindow(TaskPin taskPin)
    {
        if (taskPin == null || taskPin.data == null)
            return;

        selectedTaskPin = taskPin;
        TaskData task = taskPin.data;

        if (mapPanel != null)
            mapPanel.SetActive(false);

        if (missionPanel != null)
            missionPanel.SetActive(true);

        missaoTeste = task;

        if (missionTitleText != null)
            missionTitleText.text = task.taskName;

        if (missionDescText != null)
            missionDescText.text = task.description;

        GenerateSlotsForTask(task);

        if (minigameUI != null)
        {
            minigameUI.ResetUI();
            minigameUI.SetZoneSize(0);
        }

        OnMissionWindowOpened?.Invoke(task);
    }

    public void CloseMissionWindow()
    {
        TaskData closedTask = missaoTeste;

        missaoTeste = null;

        ReturnCrewToRoster();

        if (selectedTaskPin != null)
        {
            selectedTaskPin.ResumeTimer();
            selectedTaskPin = null;
        }

        if (missionPanel != null)
            missionPanel.SetActive(false);

        if (closedTask != null)
            OnMissionWindowClosed?.Invoke(closedTask);

        ShowMap();
    }

    private void GenerateSlotsForTask(TaskData task)
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
        if (isMissionRunning)
            return;

        List<EmployeeData> currentSquad = GetSquadFromSlots();

        if (currentSquad.Count > 0)
        {
            isMissionRunning = true;
            ConsumeSquadStamina();

            OnMissionStarted?.Invoke(missaoTeste);

            float chance = CalculateSquadChance(missaoTeste, currentSquad);

            if (minigameUI != null)
                minigameUI.SetZoneSize(chance);

            bool isCritical = chance >= 100f;
            StartCoroutine(ProcessMissionResult(chance, isCritical));
        }
        else
        {
            Debug.LogWarning("Squad vazio!");
        }
    }

    private IEnumerator ProcessMissionResult(float chancePercent, bool isCritical)
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

        float roll = UnityEngine.Random.Range(0f, 100f);
        bool isSuccess = isCritical || roll <= chancePercent;

        if (minigameUI != null)
        {
            float zoneWidth = (minigameUI.totalWidth * chancePercent) / 100f;

            if (zoneWidth >= minigameUI.totalWidth)
                zoneWidth = minigameUI.totalWidth - 1;

            float stopX = isSuccess
                ? UnityEngine.Random.Range(0f, zoneWidth)
                : UnityEngine.Random.Range(zoneWidth, minigameUI.totalWidth);

            minigameUI.StopPointer(stopX);
        }

        if (isSuccess)
        {
            Debug.Log("SUCESSO!");

            if (resourceManager != null)
            {
                resourceManager.ModifyMoney(missaoTeste.moneyReward);
                resourceManager.ModifyReputation(missaoTeste.reputationReward);
            }

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

            if (resourceManager != null)
                resourceManager.ModifyReputation(-missaoTeste.reputationPenalty);

            GiveSquadExperience(missaoTeste.xpOnFailure);
        }

        OnMissionFinished?.Invoke(missaoTeste, isSuccess, isCritical);

        yield return new WaitForSeconds(2f);

        ReturnCrewToRoster();

        if (selectedTaskPin != null)
        {
            selectedTaskPin.CompleteAndDestroy();
            selectedTaskPin = null;
        }

        ShowMap();

        missaoTeste = null;
        isMissionRunning = false;
    }

    private void GiveSquadExperience(int amount)
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

    private void ReturnCrewToRoster()
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

    private List<EmployeeData> GetSquadFromSlots()
    {
        List<EmployeeData> squad = new List<EmployeeData>();

        foreach (Slot slot in missionSlots)
        {
            if (slot.transform.childCount > 0)
            {
                EmployeeCard card = slot.transform.GetChild(0).GetComponent<EmployeeCard>();

                if (card != null && card.data != null)
                    squad.Add(card.data);
            }
        }

        return squad;
    }

    public float CalculateSquadChance(TaskData task, List<EmployeeData> squad)
    {
        if (task == null || squad == null || squad.Count == 0)
            return 0f;

        if (task.requirements == null || task.requirements.Count == 0)
            return 0f;

        float totalSquadScore = 0f;
        float totalWeights = 0f;

        foreach (TaskRequirement req in task.requirements)
        {
            if (req == null)
                continue;

            float attrSum = 0f;

            foreach (EmployeeData mem in squad)
            {
                if (mem == null)
                    continue;

                if (req.skillType == EmployeeSkillType.Cooking)
                    attrSum += mem.cookingSkill;
                else if (req.skillType == EmployeeSkillType.Service)
                    attrSum += mem.serviceSkill;
                else if (req.skillType == EmployeeSkillType.Operational)
                    attrSum += mem.operationalSkill;
                else if (req.skillType == EmployeeSkillType.Agility)
                    attrSum += mem.agility;
            }

            totalSquadScore += attrSum * req.weight;
            totalWeights += req.weight;
        }

        if (totalWeights <= 0f)
            return 0f;

        float finalScore = totalSquadScore / totalWeights;

        if (task.difficultyPoints <= 0f)
            return 100f;

        return Mathf.Clamp((finalScore / task.difficultyPoints) * 100f, 0f, 100f);
    }

    private Slot FindFirstEmptyRosterSlot()
    {
        foreach (Slot slot in rosterSlots)
        {
            if (slot.transform.childCount == 0)
                return slot;
        }

        return null;
    }

    private void SetupRosterSlots()
    {
        rosterSlots.Clear();

        foreach (Transform child in rosterContainer)
        {
            Slot slot = child.GetComponent<Slot>();

            if (slot != null)
                rosterSlots.Add(slot);
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

    private void SpawnStartingCrew()
    {
        for (int i = 0; i < startingEmployees.Count; i++)
        {
            if (i < rosterSlots.Count)
            {
                GameObject newCard = Instantiate(employeeCardPrefab, rosterSlots[i].transform);

                EmployeeCard employeeCard = newCard.GetComponent<EmployeeCard>();
                if (employeeCard != null)
                    employeeCard.Setup(startingEmployees[i]);

                Draggable draggable = newCard.GetComponent<Draggable>();
                if (draggable != null)
                    draggable.originalParent = rosterSlots[i].transform;
            }
        }
    }

    private void ConsumeSquadStamina()
    {
        int cost = missaoTeste.staminaCost;

        foreach (Slot slot in missionSlots)
        {
            if (slot.transform.childCount > 0)
            {
                EmployeeCard card = slot.transform.GetChild(0).GetComponent<EmployeeCard>();

                if (card != null)
                    card.ConsumeStamina(cost);
            }
        }
    }
}