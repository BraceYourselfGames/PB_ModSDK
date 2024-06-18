

// namespace PhantomBrigade.Data
// {
//     [ExecuteInEditMode]
//     public class DataMultiLinkerEventOutcomes : DataMultiLinker<DataContainerEventOutcome>
//     {
//         [Button]
//         public void GenerateMissingOutcomes ()
//         {
//             var eventActions = DataMultiLinkerEventActions.data.Keys;
//             var existingOutcomes = data;
//
//             foreach (var actionA in eventActions)
//             {
//                 foreach (var actionB in eventActions)
//                 {
//                     var outcomeName = actionA + actionB;
//
//                     if (existingOutcomes.ContainsKey (outcomeName))
//                     {
//                         continue;
//                     }
//
//                     var newDataContainerEventOutcome = new DataContainerEventOutcome
//                     {
//                         eventDecisionA = actionA, 
//                         eventDecisionB = actionB, 
//                         eventsOnStart = new List<string> ()
//                     };
//                     existingOutcomes.Add (outcomeName, newDataContainerEventOutcome);
//                 }
//             }
//         }
//     }
// }
