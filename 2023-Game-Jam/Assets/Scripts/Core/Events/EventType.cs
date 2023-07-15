using System;

namespace Core.Events {
    public enum EventType {
        TestEvent = 0,
        LogEvent,
        CompileDataFromMidiEvent,
        //OnNoteHit
        NoteHitNoneEvent, //to fill in param
        NoteHitEarlyEvent,
        NoteHitPerfectEvent,
        NoteHitLateEvent,
        NoteMissEvent,
        
        UnPauseEvent,
        RemoveSliderFromHoldListEvent,
        SpawnNoteNormalGround,
        SpawnNoteNormalAir,
        SpawnNoteSliderRed,
        SpawnNoteSliderBlue,
        
        SliderNoteHoldingEvent,
        SliderNoteReleaseEvent,
        
        StartSongEvent,
        LaneFinishSpawningEvent,
        PauseTransitionEvent,
        GameEndedEvent,
        EndGameTransitionEvent,
        GlobalTransitionEvent,
        GlobalTransitionCompleteEvent,
        
        UpdateStatsLevelSelectionEvent,
        
        PlayerAttack,//pass in what lane is being attacked
        PlayerBlock,
        PlayerEndBlock,

        PlayerUIUpdate,
        PlayerUIReset,
        // SideCharacterAttackGround,
        // SideCharacterAttackAir,
        // SideCharacterBlockGround,
        // SideCharacterBlockAir,
    }
}