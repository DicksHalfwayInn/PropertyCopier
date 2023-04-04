using System.Collections.ObjectModel;
using System.Reflection;

namespace ClassPropertyCopier
{
    /// <summary>
    ///    Copies matching property's values from a source class to a target class
    /// </summary>
    /// <typeparam name="TSource">The source class whose stuey property values will be copied</typeparam>
    /// <typeparam name="TTarget">The target class whose schoey property values will be overridden</typeparam>
    public static class PropertyCopier<TSource, TTarget>
        where TSource : class where TTarget : class, new()
    {
        /// <summary>
        ///    Enum for determining how/when a property's values should be copied
        /// </summary>
        public enum OverrideStatus
        {
            OverrideOnlyIfTargetIsNew,
            OverrideOnlyIfTargetPropertyValueIsNull,
            OverrideAllTargetValues,
        }

        /// <summary>
        ///    Copies Source class property values to a new Target class
        /// </summary>
        /// <param name="Tsource">The Source class to use for copying property's values</param>
        /// <returns>A new Target class with updated Property values from the Source class</returns>
        public static TTarget Copy(TSource Tsource)
        {
            // create a Target class
            TTarget Ttarget = new TTarget();

            // Get the Source and Target Properties
            PropertyInfo[] TsourcePropertyInfos = Tsource.GetType().GetProperties(); ;
            PropertyInfo[] TtargetPropertyInfos = Ttarget.GetType().GetProperties();

            // Iterate through the Source Class's Properties
            foreach (PropertyInfo TsourcePropertyInfo in TsourcePropertyInfos)
            {
                // Get the Source Class Property's Name and Value
                var TsourcePropertyName = TsourcePropertyInfo.Name;
                var TsourcePropertyValue = TsourcePropertyInfo.GetValue(Tsource, null);

                // Iterate through the Target Class's Properties
                foreach (PropertyInfo TtargetPropertyInfo in TtargetPropertyInfos)
                {
                    // If the Target property is not public / accessable... 
                    if (TtargetPropertyInfo.GetSetMethod() == null) break;

                    // If the Source Property's Name matches the Target Property's Name
                    if (TsourcePropertyName == TtargetPropertyInfo.Name)
                    {
                        // Check to see if the Source Property's Type matches the Target Property's Type
                        if (CheckToSeeIfSourcePropertyTypeMatchesTargetPropertyType(TsourcePropertyInfo, TtargetPropertyInfo))
                            // Set the Target Property's value to the Source Property's value
                            TtargetPropertyInfo.SetValue(Ttarget, TsourcePropertyValue);
                    }
                }
            }
            // Return the resultant Target class
            return Ttarget;
        }

        /// <summary>
        ///    Copies Source class property values to a new Target class with an out field indicating if any target class's property's values were changed
        /// </summary>
        /// <param name="Tsource">The Source class to use for copying property's values</param>
        /// <param name="changesMade">Flag indicating if changes were made</param>
        /// <returns>A new Target class or one that has been updated from the source</returns>
        public static TTarget Copy(TSource Tsource, out bool changesMade)
        {
            // Set the initial state for if the Target class has had any changes from New()
            changesMade = false;

            // Create a new Target class
            TTarget Ttarget = new TTarget();

            // Get the Source and Target Properties
            PropertyInfo[] TsourcePropertyInfos = Tsource.GetType().GetProperties(); ;
            PropertyInfo[] TtargetPropertyInfos = Ttarget.GetType().GetProperties();

            // Iterate through the Source Class's Properties
            foreach (PropertyInfo TsourcePropertyInfo in TsourcePropertyInfos)
            {
                // Get the Source Class Property's Name and Value
                var TsourcePropertyName = TsourcePropertyInfo.Name;
                var TsourcePropertyValue = TsourcePropertyInfo.GetValue(Tsource, null);

                // Iterate through the Target Class's Properties
                foreach (PropertyInfo TtargetPropertyInfo in TtargetPropertyInfos)
                {
                    // If the Target property is not public / accessable... 
                    if (TtargetPropertyInfo.GetSetMethod() == null) break;

                    // If the Source and Target Property's Names match...
                    if (TsourcePropertyName == TtargetPropertyInfo.Name)
                    {
                        // Check to see if the Source Property's Type matches the Target Property's Type
                        if (CheckToSeeIfSourcePropertyTypeMatchesTargetPropertyType(TsourcePropertyInfo, TtargetPropertyInfo))
                        {
                            // Set the Target Property's value to the matching Source Property's value
                            TtargetPropertyInfo.SetValue(Ttarget, TsourcePropertyValue);
                            // Change Flag to true indicating changes were made to Target class
                            changesMade = true;
                        }
                    }
                }
            }
            // Return the resultant Target class
            return Ttarget;
        }

        /// <summary>
        ///    Copies Source class property values to the Target class properties
        /// </summary>
        /// <param name="Tsource">The Source class to use for copying property's values</param>
        /// <param name="Ttarget">The Target class whose property values will be updated</param>
        /// <param name="overrideStatus">Enum for determining how/when property's values should be copied</param>
        /// <returns>The passed in Target class after updates from the Source have been made</returns>
        public static TTarget Copy(TSource Tsource, TTarget Ttarget,
            OverrideStatus overrideStatus = OverrideStatus.OverrideOnlyIfTargetIsNew)
        {
            // If the passed in Target class is null, set it to a New() Target class
            if (Ttarget == null) Ttarget = new TTarget();

            // Get the Source and Target Properties
            PropertyInfo[] TsourcePropertyInfos = Tsource.GetType().GetProperties();
            PropertyInfo[] TtargetPropertyInfos = Ttarget.GetType().GetProperties();

            // Check to see if the Target Class has been altered from New()
            var TtargetIsNew = CheckTtargetIsNew(Ttarget);

            // If the Target class is not a New Target class and you shouldn"t continue if it wasn't New()...
            if (!TtargetIsNew && overrideStatus == OverrideStatus.OverrideOnlyIfTargetIsNew)
            {
                // Return the current Target class before it is changed
                return Ttarget;
            }

            // Iterate through the Source Class's Properties
            foreach (PropertyInfo TsourcePropertyInfo in TsourcePropertyInfos)
            {
                // Get the Source Class Property's Name and Value
                var TsourcePropertyName = TsourcePropertyInfo.Name;
                var TsourcePropertyValue = TsourcePropertyInfo.GetValue(Tsource, null);

                // Iterate through the Target Class's Properties
                foreach (PropertyInfo TtargetPropertyInfo in TtargetPropertyInfos)
                {
                    // If the Target property is not public / accessable... 
                    if (TtargetPropertyInfo.GetSetMethod() == null) break;

                    // If the Source and Target Property's Names match...
                    if (TsourcePropertyName == TtargetPropertyInfo.Name)
                    {
                        // If the Target Property's value is null and that is the only reason for it to be changed...
                        if (TtargetPropertyInfo.GetValue(Ttarget, null) != null &&
                            overrideStatus != OverrideStatus.OverrideOnlyIfTargetPropertyValueIsNull)
                        {
                            // Check to see if the Source Property's Type matches the Target Property's Type
                            if (CheckToSeeIfSourcePropertyTypeMatchesTargetPropertyType(TsourcePropertyInfo, TtargetPropertyInfo))
                                // Set the Target Property's value to the Source Property's value
                                TtargetPropertyInfo.SetValue(Ttarget, TsourcePropertyValue);
                        }
                    }
                }
            }
            // Return the resultant Target class
            return Ttarget;
        }

        /// <summary>
        ///    Copies Source class property values to the Target class properties with an out field indicating if any target class's property's values were changed
        /// </summary>
        /// <param name="Tsource">The Source class to use for copying property's values</param>
        /// <param name="Ttarget">The Target class whose property values will be updated</param>
        /// <param name="changesMade">Flag indicating if changes were made</param>
        /// <param name="overrideStatus">Enum for determining how/when property's values should be copied</param>
        /// <returns>The passed in Target class after updates from the Source have been made</returns>
        public static TTarget Copy(TSource Tsource, TTarget Ttarget,
            out bool changesMade, OverrideStatus overrideStatus = OverrideStatus.OverrideOnlyIfTargetIsNew)
        {
            // If the passed in Target class is null... create a new one
            if (Ttarget == null) Ttarget = new TTarget();

            // Set the initial state for if the Target class has had any changes from New()
            changesMade = false;

            // Get the Source and Target Properties
            PropertyInfo[] TsourcePropertyInfos = Tsource.GetType().GetProperties();
            PropertyInfo[] TtargetPropertyInfos = Ttarget.GetType().GetProperties();

            // Check to see if the Target Class has been altered from New()
            var TtargetIsNew = CheckTtargetIsNew(Ttarget);

            // If the Target class is not a New Target class and it should only be overridden if it is New()...
            if (!TtargetIsNew && overrideStatus == OverrideStatus.OverrideOnlyIfTargetIsNew)
            {
                // Return the current Target class before it is changed
                return Ttarget;
            }

            // Iterate through the Source Class's Properties
            foreach (PropertyInfo TsourcePropertyInfo in TsourcePropertyInfos)
            {
                // Get the Source Class Property's Name and Value
                var TsourcePropertyName = TsourcePropertyInfo.Name;
                var TsourcePropertyValue = TsourcePropertyInfo.GetValue(Tsource, null);

                // Iterate through the Target Class's Properties
                foreach (PropertyInfo TtargetPropertyInfo in TtargetPropertyInfos)
                {
                    // If the Target property is not public / accessable... 
                    if (TtargetPropertyInfo.GetSetMethod() == null) break;

                    // If the Source and Target Property's Names match...
                    if (TsourcePropertyName == TtargetPropertyInfo.Name)
                    {
                        // If the Target Property's value is null and that is the only reason for it to be changed...
                        if (TtargetPropertyInfo.GetValue(Ttarget, null) != null &&
                            overrideStatus != OverrideStatus.OverrideOnlyIfTargetPropertyValueIsNull)
                        {
                            // Check to see if the Source Property's Type matches the Target Property's Type
                            if (CheckToSeeIfSourcePropertyTypeMatchesTargetPropertyType(TsourcePropertyInfo, TtargetPropertyInfo))
                            {
                                // Set the Target Property's value to the Source Property's value
                                TtargetPropertyInfo.SetValue(Ttarget, TsourcePropertyValue);
                                // Change Flag to true indicating changes were made to Target class
                                changesMade = true;
                            }
                        }
                    }
                }
            }
            // Return the resultant Target class
            return Ttarget;
        }

        /// <summary>
        ///    Copies a passed in List<obj> and returns a new ObservableCollection<obj> with 
        ///    propertyies with matching names values set to the List's property's values
        /// </summary>
        /// <param name="TSources">The List of classes that the collection should be copied to</param>
        /// <returns>A new List of Target class with the copied property's values</returns>
        public static ObservableCollection<TTarget> CopyListToObservableCollection(List<TSource> TSources)
        {
            // Create a new ObservableCollection of the target class
            var Ttargets = new ObservableCollection<TTarget>();

            // Iterate through the Source List Objects
            foreach (TSource Tsource in TSources)
            {
                // Create a new Target class
                var target = new TTarget();

                // Copy the Source Object's matching property's values to the new Target class
                // and add it to the new Collection of Target class
                Ttargets.Add(Copy(Tsource, target));
            }

            // Return the new Collection of Target classes
            return Ttargets;
        }

        /// <summary>
        ///    Copies a passed in ObservableCollection<obj> and returns a new List<obj> with 
        ///    propertyies with matching names values set to the Collection's property's values
        /// </summary>
        /// <param name="TSources">The Collection of classes that the List should be copied to</param>
        /// <returns>A new ObservableCollection of Target class with the copied property's values</returns>
        public static ObservableCollection<TTarget> CopyListToObservableCollection(List<TSource> sources, ObservableCollection<TTarget> targets)
        {
            // Create a new List of the target class
            var result = new ObservableCollection<TTarget>();

            // Iterate through the Source Collection Objects
            foreach (TSource source in sources)
            {
                // Create a new Target class
                var target = new TTarget();

                // Copy the Source Object's matching property's values to the new Target class
                // and add it to the new List of Target class
                result.Add(Copy(source, target));
            }

            // Return the new List of Target classes
            return result;

        }


        public static List<TTarget> CopyObservableCollectionToList(ObservableCollection<TSource> TSources)
        {
            // Create a new List of the target class
            var Ttargets = new List<TTarget>();

            // Iterate through the Source ObservableCollection Objects
            foreach (TSource Tsource in TSources)
            {
                // Create a new Target class
                var target = new TTarget();

                // Copy the Source Object's matching property's values to the new Target class
                // and add it to the new List of Target class
                Ttargets.Add(Copy(Tsource, target));
            }

            // Return the new List of Target classes
            return Ttargets;
        }

        /// <summary>
        ///    Checks if the Target class is New() or has been changed from New()
        /// </summary>
        /// <param name="Ttarget">The Target class whose property values will be updated</param>
        /// <returns>Flag indicating if the original New() values have been changed</returns>
        private static bool CheckTtargetIsNew(TTarget Ttarget)
        {
            // Set the method result to an initial value of true
            bool result = true;

            // Create a new Target object
            var newTarget = new TTarget();

            // Get the Property Information for a New() Target class
            PropertyInfo[] newTargetPropertyInfos = newTarget.GetType().GetProperties();

            // Get the Property Information for the passed in Target class
            PropertyInfo[] TtargetPropertyInfos = Ttarget.GetType().GetProperties();

            // Iterate through the Properties of the New() Target class
            foreach (PropertyInfo newTargetPropertyInfo in newTargetPropertyInfos)
            {
                // Get the Name and Value of the New() Target class
                var newTargetPropertyName = newTargetPropertyInfo.Name;
                var newTargetPropertyValue = newTargetPropertyInfo.GetValue(newTarget, null);

                // Iterate through the passed in Target class's Property information
                foreach (PropertyInfo TtargetPropertyInfo in TtargetPropertyInfos)
                {
                    // If we have properties with the same name
                    if (newTargetPropertyName == TtargetPropertyInfo.Name)
                    {
                        // If the Value of the properties are not the same as a New() Target class values...
                        if (newTargetPropertyValue != TtargetPropertyInfo.GetValue(Ttarget, null))
                            // Set the method result to false
                            result = false;
                    }
                }
            }
            // Return Flag indicating if the passed in Target class's properties don't match a New() version of the same class
            return result;
        }

        /// <summary>
        ///    Checks to see if the Source's Property.Name's Type matches the Target's Property's Name's Type
        /// </summary>
        /// <param name="TsourcePropertyInfo">The Source's Property information</param>
        /// <param name="TtargetPropertyInfo">The Target's Property information</param>
        /// <returns>Flag indicating if the Source Property's Type matches the Target Property's Type</returns>
        private static bool CheckToSeeIfSourcePropertyTypeMatchesTargetPropertyType(PropertyInfo TsourcePropertyInfo, PropertyInfo TtargetPropertyInfo)
        {
            // Return True if the two passed in Property's Types match
            return TsourcePropertyInfo.PropertyType == TtargetPropertyInfo.PropertyType;
        }
    }
}
