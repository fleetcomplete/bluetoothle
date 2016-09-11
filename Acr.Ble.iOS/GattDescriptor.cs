﻿using System;
using System.Reactive.Linq;
using CoreBluetooth;
using Foundation;


namespace Acr.Ble
{
    public class GattDescriptor : AbstractGattDescriptor
    {
        readonly CBDescriptor native;


        public GattDescriptor(IGattCharacteristic characteristic, CBDescriptor native) : base(characteristic, native.UUID.ToGuid())
        {
            this.native = native;
        }


        public override IObservable<byte[]> Read()
        {
            return Observable.Create<byte[]>(ob =>
            {
                var p = this.native.Characteristic.Service.Peripheral;

                var handler = new EventHandler<CBDescriptorEventArgs>((sender, args) =>
                {
                    if (args.Descriptor.UUID.Equals(this.native.UUID))
                    {
                        if (args.Error != null)
                            ob.OnError(new ArgumentException(args.Error.ToString()));
                        else
                        {
                            this.Value = ((NSData)args.Descriptor.Value).ToArray();
                            ob.OnNext(this.Value);
                            ob.OnCompleted();
                        }
                    }
                });
                p.UpdatedValue += handler;
                p.ReadValue(this.native);
                return () => p.UpdatedValue -= handler;
            });
        }


        public override IObservable<object> Write(byte[] data)
        {
            return Observable.Create<object>(ob =>
            {
                var p = this.native.Characteristic.Service.Peripheral;

                var handler = new EventHandler<CBDescriptorEventArgs>((sender, args) =>
                {
                    if (args.Descriptor.UUID.Equals(this.native.UUID))
                    {
                        if (args.Error != null)
                            ob.OnError(new ArgumentException(args.Error.ToString()));
                        else
                        {
                            this.Value = data;
                            ob.OnCompleted();
                        }
                    }
                });

                p.WroteDescriptorValue += handler;
                var nsdata = NSData.FromArray(data);
                p.WriteValue(nsdata, this.native);

                return () => p.WroteDescriptorValue -= handler;
            });
        }
    }
}