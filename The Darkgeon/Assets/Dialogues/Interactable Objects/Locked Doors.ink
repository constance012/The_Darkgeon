INCLUDE ../Variables Persistence/Globals.ink

{is_door_opened:
    -> unlocked_door
  - else:
    -> locked_door
}

-> END

=== locked_door ===
    # Layout: narrator_layout
    It's locked.
    
    * [Pry it open] You tried prying the door open with your sword.
        # Layout: speaker_layout # Speaker: You # Portrait: warrior
        That didn't work, my sword is gonna bend.
        Need to find some sort of <color=\#e6b800>keys</color> or <color=\#e6b800>mechanisms</color> to unlock it. Probably somewhere around here.
        -> DONE
        
    * [Leave] -> DONE
    
=== unlocked_door ===
    # Layout: speaker_layout # Speaker: You # Portrait: warrior
    Oh, seems like it's controlled by those levers.
    
    # Layout: narrator_layout
    You shoved the heavy wooden door opened, and took a look inside.
    The path was dimly lit by torches, the flickering fire occasionally dropped its embers down the damp floor
    There was a stair leading down, you couldn't tell what's down there.
    
    # Layout: speaker_layout # Speaker: You # Portrait: warrior
    It's kinda dark despite the torches, and there might be <color=\#e6b800>monsters' presence</color>...
    
    * [Enter]
        ...But my sword is ready for them.
        ~ ExternalFunction()
        -> DONE
    
    * [Leave]
        ...I'll prepair and come back later
        -> DONE
    
    