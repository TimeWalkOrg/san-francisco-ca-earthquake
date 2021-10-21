using Normal.Realtime;

public class Stopwatch : RealtimeComponent<StopwatchModel> {
    public float time {
        get {
            // Return 0 if we're not connected to the room yet.
            if (model == null) return 0.0f;

            // Make sure the stopwatch is running
            if (model.startTime == 0.0) return 0.0f;

            // Calculate how much time has passed
            return (float)(realtime.room.time - model.startTime);
        }
    }

    public void StartStopwatch() {
        //You can retart the timer by calling StartStopwatch again
        if (realtime.room != null) {
            model.startTime = realtime.room.time;
            
        }
        else {
            print("Room does not exist");
            model.startTime = 0f;
        }
    }

    public bool CheckRoom() {
        if (realtime.room != null) {
            return true;
        }
        else {
            return false;
        }
    }

}