// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------

using System;
using UnityEngine;

namespace Realtime.LITJson
{
    public class UnityExtensions
    {
        public static void AddExtensions()
        {
            // Vector4 exporter
            ExporterFunc<Vector4> vector4Exporter = Vector4Exp;
            JsonMapper.RegisterExporter(vector4Exporter);

            // Vector3 exporter
            ExporterFunc<Vector3> vector3Exporter = Vector3Exp;
            JsonMapper.RegisterExporter(vector3Exporter);

            // Vector2 exporter
            ExporterFunc<Vector2> vector2Exporter = Vector2Exp;
            JsonMapper.RegisterExporter(vector2Exporter);

            // Quaternion exporter
            ExporterFunc<Quaternion> quaternionExporter = QuaternionExp;
            JsonMapper.RegisterExporter(quaternionExporter);
        
            // Color exporter
            ExporterFunc<Color> colorExporter = ColorExp;
            JsonMapper.RegisterExporter(colorExporter);

            // float to double
            ExporterFunc<float> float2Double = Float2Double;
            JsonMapper.RegisterExporter(float2Double);

            // double to float
            ImporterFunc<double, Single> double2Float = Double2Float;
            JsonMapper.RegisterImporter(double2Float);
        }

        public static void Vector4Exp(Vector4 value, JsonWriter writer)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName("x");
            writer.Write(value.x);
            writer.WritePropertyName("y");
            writer.Write(value.y);
            writer.WritePropertyName("z");
            writer.Write(value.z);
            writer.WritePropertyName("w");
            writer.Write(value.w);
            writer.WriteObjectEnd();
        }

        public static void Vector3Exp(Vector3 value, JsonWriter writer)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName("x");
            writer.Write(value.x);
            writer.WritePropertyName("y");
            writer.Write(value.y);
            writer.WritePropertyName("z");
            writer.Write(value.z);
            writer.WriteObjectEnd();
        }

        public static void Vector2Exp(Vector2 value, JsonWriter writer)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName("x");
            writer.Write(value.x);
            writer.WritePropertyName("y");
            writer.Write(value.y);
            writer.WriteObjectEnd();
        }

        public static void ColorExp(Color value, JsonWriter writer)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName("r");
            writer.Write(value.r);
            writer.WritePropertyName("b");
            writer.Write(value.b);
            writer.WritePropertyName("g");
            writer.Write(value.g);
            writer.WritePropertyName("a");
            writer.Write(value.a);
            writer.WriteObjectEnd();
        }
        public static void QuaternionExp(Quaternion value, JsonWriter writer)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName("x");
            writer.Write(value.x);
            writer.WritePropertyName("y");
            writer.Write(value.y);
            writer.WritePropertyName("z");
            writer.Write(value.z);
            writer.WritePropertyName("w");
            writer.Write(value.w);
            writer.WriteObjectEnd();
        }
        public static void Float2Double(float value, JsonWriter writer)
        {
            writer.Write(value);
        }

        public static Single Double2Float(double value)
        {
            return (Single) value;
        }
    }
}