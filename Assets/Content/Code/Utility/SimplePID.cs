using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable][HideReferenceObjectPicker]
public class SimplePIDSettings
{
    // [Tooltip ("This is the point we want to reach")]
    // public float setPoint = 0f;

    [TableList]
    [Tooltip ("Proportional gain is tweaked first: it's how strongly you correct, based on how far off the target you are. The problem with only using P, is that you might never reach your goal. Take the example of two cars following eachother on the highway. As you get closer you ease off the gas. You'll never actually catch up to them.")]
    [InlineButton ("@proportionalGain = 0.05f", "Reset")]
    public float proportionalGain = 0f;

    [TableList]
    [Tooltip ("Derivative gain is tweaked second: it controls how sensitive the controller is to rate of change. For example, if you're catching up on that car, and slowing down because you're getting closer, this can compensate for that, by applying more gas, since the rate of change is slowing down. You'll essentially \"catch up\". In cruise control it also can account for hills, since it takes more gas to maintain your speed.")]
    [InlineButton ("@derivativeGain = 0.001f", "Reset")]
    public float derivativeGain = 0f;

    [TableList]
    [Tooltip ("Integral gain is tweaked third: it is keeping track over time of how long you've been off your target at a given side, and pushes the output to return to 0. You could for example, spin a dial around any number of times, and have it remember by how much in total it's off from its target rotation. It could then unwind all the way back as many rotations to the original position as needed.")]
    [InlineButton ("@integralGain = 0f", "Reset")]
    public float integralGain = 0f;
}

[Serializable][HideReferenceObjectPicker]
public class SimplePID
{
    public SimplePIDSettings settings;
    
    // [Tooltip ("This is the point we want to reach")]
    // public float setPoint = 0f;

    //PID parameters, each controls how much power each effect has on the end result

    // [Tooltip ("Proportional gain is tweaked first: it's how strongly you correct, based on how far off the target you are. The problem with only using P, is that you might never reach your goal. Take the example of two cars following eachother on the highway. As you get closer you ease off the gas. You'll never actually catch up to them.")]
    // public float proportionalGain = 0f;

    // [Tooltip ("Derivative gain is tweaked second: it controls how sensitive the controller is to rate of change. For example, if you're catching up on that car, and slowing down because you're getting closer, this can compensate for that, by applying more gas, since the rate of change is slowing down. You'll essentially \"catch up\". In cruise control it also can account for hills, since it takes more gas to maintain your speed.")]
    // public float derivativeGain = 0f;

    // [Tooltip ("Integral gain is tweaked third: it is keeping track over time of how long you've been off your target at a given side, and pushes the output to return to 0. You could for example, spin a dial around any number of times, and have it remember by how much in total it's off from its target rotation. It could then unwind all the way back as many rotations to the original position as needed.")]
    // public float integralGain = 0f;


    // Total accumulated error;
    private float integral = 0;
    private float previousError = 0;

    /// <summary>
    /// The signal is what we are trying to correct for, and delta time is how much time has passed between the last time we corrected
    /// </summary>
    /// <param name="errorSignal"></param>
    /// <param name="deltaTime"></param>
    /// <returns></returns>
    public float GetCorrection (float errorSignal, float deltaTime)
    {
        if (settings == null)
            return 0f;

        float setPoint = 0f; // settings.setPoint;
        float error = setPoint - errorSignal;

        // Integral term calculation, this is the error over time, as it piles up:
        // this is really useful for things that you want to "catch up" or "remember",
        // and you can safely set the gain of this to 0 and it will be ignored
        integral += error * deltaTime;

        // Derivative term calculation, rate of change, useful for smoothly arresting right on target:
        // you can also set this to 0 to ignore it if you like
        float currentDerivative = errorSignal - previousError;
        float derivativeTerm = currentDerivative / deltaTime;

        // Proportional term calculation, how aggressively we correct based on how far off we are:
        // this is the main tuning parameter
        float proportionalTerm = error;

        // The output is how the PID controller thinks we should respond to our current error
        float output = 
            (proportionalTerm * settings.proportionalGain) + 
            (integral * settings.integralGain) - 
            (derivativeTerm * settings.derivativeGain);
        
        previousError = error;
        return output;
    }

    // Clears accumlated errors if needed, and resets the controller
    public void Reset ()
    {
        integral = 0;
        previousError = 0;
    }
}