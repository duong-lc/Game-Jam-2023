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
        SpawnNoteSlider,
        
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
        
        PlayerAttackGround,
        PlayerAttackAir,
        PlayerHurt,
        PlayerJump,
        PlayerLand,
        PlayerBlockGround,
        PlayerBlockAir,
        RequestIsGrounded,
        ReceiveIsGrounded,
        RequestMCLane,
        ReceiveMCLane,
        
        SideCharacterAttackGround,
        SideCharacterAttackAir,
        SideCharacterBlockGround,
        SideCharacterBlockAir,
    }
}