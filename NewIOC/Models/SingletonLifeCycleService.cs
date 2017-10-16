﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewIOC.Models
{
    public class SingletonLifeCycleService : ILifeCycleService
    {
        private readonly IDictionary<Type, Component> _registry = new Dictionary<Type, Component>();
        public void RegisterComponent(Type typeKey, Component component)
        {
            if (!_registry.ContainsKey(typeKey))
            {
                _registry.Add(typeKey, component);
            }
            else
            {
                //Override any existing component for now
                _registry[typeKey] = component;
            }
        }

        public object ResolveInstance(Type typeKey)
        {
            if (_registry.ContainsKey(typeKey))
            {
                var component = _registry[typeKey];
                if (component.Instance == null)
                {
                    return CreateInstance(component.ConcreteType);
                }

                return CreateInstance(component.ConcreteType);
            }

            throw new ImplementationNotFoundException($"Cannot find implementation for {typeKey.FullName}");
        }
        
        private object CreateInstance(Type concreteType)
        {
            var args = new List<object>();

            var constructor = concreteType.GetConstructors().FirstOrDefault();
            var parameterList = constructor.GetParameters();

            if (!parameterList.Any())
            {
                return Activator.CreateInstance(concreteType);
            }

            //Recursive appraoch to instantiate a nested type that depends on other registered types
            foreach (var parameter in parameterList)
            {
                //if (paramType.IsInterface)
                var resolvedParameterObject = ResolveInstance(parameter.ParameterType);
                args.Add(resolvedParameterObject);
                //else
                //Trust that constructor arguments are interfaces only.
                //Why should they use DI when constructors take concrete types?
                //DI entails following a convention over configuration approach.


                //Catch here for possible circular object graph of dependencies
            }


            var instance = Activator.CreateInstance(concreteType, args.ToArray());
            Console.WriteLine($"Type : {instance.GetType()}");
            return instance;
        }
    }
}