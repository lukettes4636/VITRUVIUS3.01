using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDialogueData", menuName = "Dialogue System/Player Dialogue Data")]
public class PlayerDialogueData : ScriptableObject
{
    public string flashlightOtherReact_Player1 = "Hey, share the light!";
    public string flashlightOtherReact_Player2 = "Nice, you got it. I still need one.";
    public string flashlightBothHave_Player2WasLast_Player1Teasing = "Took you long enough, rookie.";
    public string flashlightBothHave_Player1WasLast_Player2Encouraging = "Great, now you're set too!";

    public string keyCardCollector_Player1 = "Keycard acquired.";
    public string keyCardOtherReact_Player2OnP1 = "Good job, I'll cover you.";
    public string keyCardCollector_Player2 = "Keycard secured.";
    public string keyCardOtherReact_Player1OnP2 = "Alright, let's move.";

    public string leverCollector_Player1 = "Lever picked.";
    public string leverOtherReact_Player2OnP1 = "Flip it when ready.";
    public string leverCollector_Player2 = "Lever obtained.";
    public string leverOtherReact_Player1OnP2 = "We might need that soon.";

    public string keyCollector_Player1 = "Key found.";
    public string keyOtherReact_Player2OnP1 = "Perfect, that'll open something.";
    public string keyCollector_Player2 = "Key collected.";
    public string keyOtherReact_Player1OnP2 = "Nice, keep it handy.";

    public string monitorComputer_First_Player1 = "Computer looks important.";
    public string monitorComputer_First_Player2 = "I'll check this terminal.";
    public string monitorComputer_Second_Player1WhenP2First = "You got here first. What do you see?";
    public string monitorComputer_Second_Player2WhenP1First = "On it. Let's scan fast.";

    public string monitorNote_First_Player1 = "A note on the wall.";
    public string monitorNote_First_Player2 = "There's a note here.";
    public string monitorNote_Second_Player1WhenP2First = "Read it aloud.";
    public string monitorNote_Second_Player2WhenP1First = "Let's decode it.";

    public string keypadEnter_First_Player1 = "I'll try the code.";
    public string keypadEnter_First_Player2 = "Let me handle this keypad.";
    public string keypadEnter_Second_Player1WhenP2First = "I'll watch your back.";
    public string keypadEnter_Second_Player2WhenP1First = "Keep trying. I'll cover you.";

    public string keypadCode_Incorrect_OtherReact_Player2OnP1Attempt = "Wrong code. Try again.";
    public string keypadCode_Incorrect_OtherReact_Player1OnP2Attempt = "No match. Think it through.";
    public string keypadCode_Correct_OtherReact_Player2OnP1Attempt = "Nice. Door should unlock.";
    public string keypadCode_Correct_OtherReact_Player1OnP2Attempt = "Great entry. Let's move.";

    public string warningDoor_Enter_Player1 = "We need to cut the power.";
    public string warningDoor_Enter_Player2 = "Barrier's active. Power first.";

    public string electricBox_Enter_HasLever_Player1 = "I've got the lever part.";
    public string electricBox_Enter_NoLever_Player1 = "Missing the lever part.";
    public string electricBox_Enter_HasLever_Player2 = "Lever ready here.";
    public string electricBox_Enter_NoLever_Player2 = "We still need the lever.";

    public string keyDoor_Enter_HasKey_Player1 = "I've got the key.";
    public string keyDoor_Enter_NoKey_Player1 = "No key yet.";
    public string keyDoor_Enter_HasKey_Player2 = "Key on me.";
    public string keyDoor_Enter_NoKey_Player2 = "We need the key.";

    public string enemyBreakWall_Player1 = "It broke through! Move!";
    public string enemyBreakWall_Player2 = "Wall's down! Run!";
    public string enemyChaseEnded_Player1 = "Lost it. Keep low.";
    public string enemyChaseEnded_Player2 = "It's gone. Stay alert.";
    public string enemyDetectedAgain_Player1 = "It saw us again!";
    public string enemyDetectedAgain_Player2 = "We've been spotted!";
    public string survivorAfterDeath_Player1 = "I'll finish this... for you.";
    public string survivorAfterDeath_Player2 = "I'll carry on. Hold on.";

    public string fallenDoor_Enter_Player1_Confident = "I'll get this door up.";
    public string fallenDoor_Enter_Player2_Attempt = "I'll try lifting it.";
    public string fallenDoor_LiftFail_Player2 = "I can't lift this.";
    public string fallenDoor_LiftFail_Response_Player1 = "Let me try.";
    public string fallenDoor_LiftSuccess_Player2 = "Great, now I can pass!";
    public string fallenDoor_EarlyRelease_Player1 = "lo intentare de nuevo";
    public string fallenDoor_EarlyRelease_Player2 = "lo intentare de nuevo";

    public string doubleDoor_SingleEnter_Player1 = "I need help with this door!";
    public string doubleDoor_SingleEnter_Player2 = "I need help with this door!";
    public string doubleDoor_SingleEnter_Response_Player2OnP1 = "On my way.";
    public string doubleDoor_SingleEnter_Response_Player1OnP2 = "Coming.";
    public string doubleDoor_BothReady_Player1 = "Ready. Hit together.";
    public string doubleDoor_BothReady_Player2 = "Ready here. Together.";
    public string doubleDoor_FinalHitApproach_Player1 = "One more. Together.";
    public string doubleDoor_FinalHitApproach_Player2 = "Last hit. Go.";
    public string doubleDoor_Opened_Player1 = "Door opened!";
    public string doubleDoor_Opened_Player2 = "It's open!";
}
