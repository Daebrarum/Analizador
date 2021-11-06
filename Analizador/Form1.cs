using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Analizador
{
    public partial class Form1 : Form
    {
        List<double> pilaNumeros = new List<double>();
        int contadorNumeros = 0;
        List<char> pilaOperadores = new List<char>();
        int contadorOperadores = 0;
        List<string> listaPalabrasReservadas = new List<string>();
        Dictionary<string, double> listaVariables = new Dictionary<string, double>();
        string strOperadores = "/*-+();";
        string caracteresLogicos = "=<>";
        string []lineas;
        public Form1()
        {
            InitializeComponent();
            listaPalabrasReservadas.Add("if");
            listaPalabrasReservadas.Add("while");
        }

        private int analizar(int inicioAnalisis)
        {
            string[] lineaAuxiliar;
            int numeroLineaActual = inicioAnalisis;
            int numeroLineasTotal = lineas.Length;
            string linea = lineas[numeroLineaActual].Trim();
            string palabraReservada;
            while(numeroLineaActual < numeroLineasTotal && !linea.Contains("}"))
            {
                if (linea.Length > 0 && linea.EndsWith(';') || linea.EndsWith('{') && linea.Length > 1)
                {
                    palabraReservada = contienePalabraReservada(linea);
                    if (palabraReservada != null) 
                    {
                        numeroLineaActual = hasPalabraReservada(numeroLineaActual, palabraReservada); 
                    }
                    else
                    {
                        if (linea.Contains('='))
                        {
                            lineaAuxiliar = linea.Split('=');
                            if (lineaAuxiliar[1].Contains('+') || lineaAuxiliar[1].Contains('-') ||
                                lineaAuxiliar[1].Contains('*') || lineaAuxiliar[1].Contains('/') ||
                                lineaAuxiliar[1].Contains('(') || lineaAuxiliar[1].Contains(')'))
                            {
                                lineaAuxiliar[1] = realizaExpresion(lineaAuxiliar[1]) + ";";
                            }
                                agregaVariable(lineaAuxiliar);
                        }
                        else
                        {
                            if (!contieneIncrementadorODecrementador(linea))
                            {
                                consola.Items.Add("resultado de la expresion:" + realizaExpresion(linea));
                            }
                        }
                    }
                }
                if (numeroLineaActual != numeroLineasTotal-1) numeroLineaActual++;
                linea = lineas[numeroLineaActual].Trim();
            }
            return numeroLineaActual;
        }
        //comprueba que existan el mismo numero de llaves de apertura como de cierre
        private bool verificarLlaves()
        {
            int contadorLlaves = 0;
            bool inicioCorrecto = false;
            if (lineas[0].Contains('{')) inicioCorrecto = true;
            foreach (string renglon in lineas)
            {
                if (renglon.Trim().Contains('{'))
                {
                    contadorLlaves++;
                }
                if (renglon.Trim().Contains('}'))
                {
                    contadorLlaves--;
                }
            }
            return contadorLlaves == 0 && inicioCorrecto;
        }

        //verifica si hay una palabra reservada en la cadena
        private string contienePalabraReservada(string linea)
        {
            string palabra = "";
            int posicionCaracter = 0;
            while (linea[posicionCaracter] != '('
                && linea[posicionCaracter] != ' '
                && posicionCaracter < linea.Length - 1)
            {
                palabra = palabra + linea[posicionCaracter];
                posicionCaracter++;
            }
            if (listaPalabrasReservadas.Contains(palabra))
            {
                return palabra;
            }
            return null;
        }

        //realiza la accion dependiendo que palabra reservada es
        private int hasPalabraReservada(int numeroLineaActual, string palabraReservada)
        {
            switch (palabraReservada)
            {
                case "if":
                    numeroLineaActual = realizaIf(numeroLineaActual);
                    break;
                case "while":
                    numeroLineaActual = realizaWhile(numeroLineaActual);
                    break;
            }
            return numeroLineaActual;
        }

        //realiza la expresion dada por la cadena
        private double realizaExpresion(string cadenaExpresion)
        {
            pilaOperadores.Clear();
            pilaNumeros.Clear();
            contadorNumeros = 0;
            contadorOperadores = 0;
            string cadenaAuxiliar = "";
            bool tipoOperacion = false;
            for (int posicionCaracter = 0; posicionCaracter < cadenaExpresion.Length; posicionCaracter++)
            {
                if (strOperadores.Contains(cadenaExpresion[posicionCaracter]))
                {
                    if (cadenaAuxiliar.Trim().Length > 0)
                    {
                        if (listaVariables.Keys.Contains(cadenaAuxiliar.Trim()))
                        {
                            pilaNumeros.Add(listaVariables[cadenaAuxiliar.Trim()]);
                        }
                        else
                        {
                            pilaNumeros.Add(double.Parse(cadenaAuxiliar));
                        }
                        cadenaAuxiliar = "";
                        contadorNumeros++;
                        muestraResumen();
                    }
                    switch (cadenaExpresion[posicionCaracter])
                    {
                        case '*':
                        case '/':
                            tipoOperacion = (contadorOperadores > 0 &&
                                (pilaOperadores[contadorOperadores - 1] == '/' ||
                                pilaOperadores[contadorOperadores - 1] == '*'));
                            agregarOperador(cadenaExpresion[posicionCaracter], tipoOperacion);
                            break;
                        case '+':
                        case '-':
                            tipoOperacion = (contadorOperadores > 0 &&
                                (pilaOperadores[contadorOperadores - 1] == '/' ||
                                 pilaOperadores[contadorOperadores - 1] == '*' ||
                                 pilaOperadores[contadorOperadores - 1] == '+' ||
                                 pilaOperadores[contadorOperadores - 1] == '-'));
                            agregarOperador(cadenaExpresion[posicionCaracter], tipoOperacion);
                            break;
                        case '(':
                            pilaOperadores.Add('(');
                            contadorOperadores++;
                            muestraResumen();
                            break;
                        case ')':
                            while (pilaOperadores[contadorOperadores - 1] != '(')
                            {
                                hasOperacion();
                            }
                            contadorOperadores--;
                            pilaOperadores.RemoveAt(contadorOperadores);
                            muestraResumen();
                            break;
                        case ';':
                            while (contadorOperadores != 0)
                            {
                                hasOperacion();
                            }
                            break;
                    }
                }
                else
                {
                    cadenaAuxiliar = cadenaAuxiliar + cadenaExpresion[posicionCaracter];
                }
            }
            return pilaNumeros[0];
        }


        /*
         metodos & funciones que no forman parte del orden del analizador pero son llamados en determinado momento
         */
        //imprime todas las pilas que utlizan (variables, numeros, operadores)
        private void muestraResumen()
        {
            pasos.Items.Add("---------------------------------");
            string aux = "[  ";
            for (int i = 0; i < contadorNumeros; i++)
            {
                aux = aux + "  " + pilaNumeros[i];
            }
            aux = aux + "  ]";
            pasos.Items.Add("Numeros: " + aux);

            aux = "[  ";
            for (int i = 0; i < contadorOperadores; i++)
            {
                aux = aux + "  " + pilaOperadores[i];
            }
            aux = aux + "  ]";
            pasos.Items.Add("Operadores: " + aux);

            aux = "[ ";
            foreach (var item in listaVariables)
            {
                aux = aux + " " + item.Key + "->" + item.Value + "   ";
            }
            aux = aux + "  ]";
            pasos.Items.Add("Variables: " + aux);
            pasos.Items.Add("---------------------------------");

            pasos.TopIndex = pasos.Items.Count - 1;
        }

        //realiza un tipo de operacion entre dos numeros (+-*/)
        private void hasTipoOperacion()
        {
            if (pilaOperadores[contadorOperadores - 1] == '*')
            {
                pasos.Items.Add("Operacion: " + pilaNumeros[contadorNumeros - 1] + "*" + pilaNumeros[contadorNumeros]);
                pilaNumeros[contadorNumeros - 1] = pilaNumeros[contadorNumeros - 1] * pilaNumeros[contadorNumeros];
            }
            if (pilaOperadores[contadorOperadores - 1] == '/')
            {
                pasos.Items.Add("Operacion: " + pilaNumeros[contadorNumeros - 1] + "/" + pilaNumeros[contadorNumeros]);
                pilaNumeros[contadorNumeros - 1] = pilaNumeros[contadorNumeros - 1] / pilaNumeros[contadorNumeros];
            }
            if (pilaOperadores[contadorOperadores - 1] == '+')
            {
                pasos.Items.Add("Operacion: " + pilaNumeros[contadorNumeros - 1] + "+" + pilaNumeros[contadorNumeros]);
                pilaNumeros[contadorNumeros - 1] = pilaNumeros[contadorNumeros - 1] + pilaNumeros[contadorNumeros];
            }
            if (pilaOperadores[contadorOperadores - 1] == '-')
            {
                pasos.Items.Add("Operacion: " + pilaNumeros[contadorNumeros - 1] + "-" + pilaNumeros[contadorNumeros]);
                pilaNumeros[contadorNumeros - 1] = pilaNumeros[contadorNumeros - 1] - pilaNumeros[contadorNumeros];
            }
        }

        //reduce las pilas al realizar una operacion
        private void hasOperacion()
        {
            contadorNumeros--;
            hasTipoOperacion();
            contadorOperadores--;
            pilaNumeros.RemoveAt(contadorNumeros);
            pilaOperadores.RemoveAt(contadorOperadores);
            muestraResumen();
        }

        //cambia o agrega un operador
        private void agregarOperador(char operadorNuevo, bool tipoOperacion)
        {
            if (tipoOperacion)
            {
                contadorNumeros--;
                hasTipoOperacion();
                pilaOperadores[contadorOperadores - 1] = operadorNuevo;
                pilaNumeros.RemoveAt(contadorNumeros);
            }
            else
            {
                pilaOperadores.Add(operadorNuevo);
                contadorOperadores++;
            }
            muestraResumen();
        }

        //separa las partes de la condicion
        private string[] separaCadenaCondicion(string cadenaCondicion)
        {
            string[] partesCondicion = new string[3];
            int posicionCaracter = 0;
            while (!caracteresLogicos.Contains(cadenaCondicion[posicionCaracter]))
            {
                partesCondicion[0] = partesCondicion[0] + cadenaCondicion[posicionCaracter];
                posicionCaracter++;
            }
            partesCondicion[0] = partesCondicion[0].Trim();
            while (caracteresLogicos.Contains(cadenaCondicion[posicionCaracter]))
            {
                partesCondicion[1] = partesCondicion[1] + cadenaCondicion[posicionCaracter];
                posicionCaracter++;
            }
            partesCondicion[1] = partesCondicion[1].Trim();
            while (posicionCaracter < cadenaCondicion.Length &&
                cadenaCondicion[posicionCaracter] != ')' && 
                cadenaCondicion[posicionCaracter] != '&' &&
                cadenaCondicion[posicionCaracter] != '|')
            {
                partesCondicion[2] = partesCondicion[2] + cadenaCondicion[posicionCaracter];
                posicionCaracter++;
            }
            partesCondicion[2] = partesCondicion[2].Trim();
            return partesCondicion;
        }

        //regresa un arreglo que contiene todas las partes de las dos condiciones y las devuelve junto con el operador logico
        private string[] desestructurarCadenaCondicion(string cadenaCondicion)
        {
            string[] partesCondicion;
            string operadorLogico;
            if (cadenaCondicion.Contains("&&") || cadenaCondicion.Contains("||"))
            {
                partesCondicion = new string[7];
                if (cadenaCondicion.Contains("&&"))
                    operadorLogico = "&&";
                else
                    operadorLogico = "||";
                string[] cadenasCondicion = cadenaCondicion.Split(operadorLogico);
                string[] arregloAuxiliar = separaCadenaCondicion(cadenasCondicion[0]);
                partesCondicion[0] = arregloAuxiliar[0];
                partesCondicion[1] = arregloAuxiliar[1];
                partesCondicion[2] = arregloAuxiliar[2];
                arregloAuxiliar = separaCadenaCondicion(cadenasCondicion[1]);
                partesCondicion[3] = arregloAuxiliar[0];
                partesCondicion[4] = arregloAuxiliar[1];
                partesCondicion[5] = arregloAuxiliar[2];
                partesCondicion[6] = operadorLogico;
            }
            else
            {
                partesCondicion = separaCadenaCondicion(cadenaCondicion);
            }
            return partesCondicion;

        }

        //retorna el resultado de una operacion logica
        private bool verificaCondicion(string valor1, string operadorLogico, string valor2)
        {
            double v1 = 0, v2 = 0;
            if (listaVariables.Keys.Contains(valor1)) v1 = listaVariables[valor1];
            else v1 = double.Parse(valor1);
            if (listaVariables.Keys.Contains(valor2)) v2 = listaVariables[valor2];
            else v2 = double.Parse(valor2);
            switch (operadorLogico)
            {
                case "<":
                    return (v1 < v2);
                case ">":
                    return (v1 > v2);
                case "==":
                    return (v1 == v2);
                case "<=":
                    return (v1 <= v2);
                case ">=":
                    return (v1 >= v2);
                case "<>":
                    return (v1 != v2);
            }
            return false;
        }

        //retorna true o false de una linea que puede contener dos condiciones logicas o una sola
        private bool comprobarCondiciones(string []partesCadenaCondiciones)
        {
            bool resultadoCondicion = verificaCondicion(partesCadenaCondiciones[0],   //valor 1
                                         partesCadenaCondiciones[1],                    //operador logico
                                         partesCadenaCondiciones[2]);                   //valor 2
            bool condicionAuxiliar = false;
            if (partesCadenaCondiciones.Length == 7)
            {
                condicionAuxiliar = verificaCondicion(partesCadenaCondiciones[3],
                                                      partesCadenaCondiciones[4],
                                                      partesCadenaCondiciones[5]);
                if (partesCadenaCondiciones[6].Equals("&&"))
                {
                    resultadoCondicion = resultadoCondicion && condicionAuxiliar;
                }
                else
                {
                    resultadoCondicion = resultadoCondicion || condicionAuxiliar;
                }
            }
            return resultadoCondicion;
        }

        //si no se cumple una condicion se avanza hasta el cierre de la condicion
        private int saltarcodigo(int numeroActualLinea)
        {
            int contadorllaves = 0;
            if (lineas[numeroActualLinea].Contains('{'))
            {
                contadorllaves++;
                numeroActualLinea++;
            }
            do
            {
                if (lineas[numeroActualLinea].Contains('{')) contadorllaves++;
                if (lineas[numeroActualLinea].Contains('}')) contadorllaves--;
                numeroActualLinea++;
            } while (contadorllaves != 0);
            return numeroActualLinea;
        }

        //agrega una nueva variable con su valor
        private void agregaVariable(string []partesLinea)
        {
            double valor = double.Parse(partesLinea[1].Substring(0, partesLinea[1].Length - 1));
            if (listaVariables.Keys.Contains(partesLinea[0].Trim()))
            {
                listaVariables[partesLinea[0].Trim()] = valor;
            }
            else
            {
                listaVariables.Add(partesLinea[0].Trim(), valor);
            }
            muestraResumen();
        }

        //verifica si la linea solo incrementa una variable en 1 
        private bool contieneIncrementadorODecrementador(string linea)
        {
            if(linea.Length > 3)
            {
                string nombreVariable = linea.Substring(0, linea.Length - 3);
                if (linea.Substring(linea.Length - 3, 2).Equals("++"))
                {
                    listaVariables[nombreVariable] = listaVariables[nombreVariable] + 1;
                    return true;
                }
                if (linea.Substring(linea.Length - 3, 2).Equals("--"))
                {
                    listaVariables[nombreVariable] = listaVariables[nombreVariable] - 1;
                    return true;
                }
            }
            return false;
        }

        //hace la accion del if
        private int realizaIf(int numeroLineaActual)
        {
            string cadenaCondicion = lineas[numeroLineaActual].Split('(')[1];
            string[] partesCondicion = desestructurarCadenaCondicion(cadenaCondicion);
            if (comprobarCondiciones(partesCondicion))
            {
                numeroLineaActual = analizar(numeroLineaActual + 1);
            }
            else
            {
                numeroLineaActual = saltarcodigo(numeroLineaActual);
            }
            return numeroLineaActual;
        }

        //hace la accion del while
        private int realizaWhile(int numeroLineaActual)
        {
            string cadenaCondicion = lineas[numeroLineaActual].Split('(')[1];
            string[] partesCondicion = desestructurarCadenaCondicion(cadenaCondicion);
            int numeroLineaAuxiliar = numeroLineaActual;
            while (comprobarCondiciones(partesCondicion)) { 
                numeroLineaActual = analizar(numeroLineaAuxiliar + 1);
            }
            if (numeroLineaActual == numeroLineaAuxiliar) numeroLineaActual = saltarcodigo(numeroLineaActual);
            return numeroLineaActual;
        }


        //metodos de control

        private void analizarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pasos.Items.Clear();
            pilaOperadores.Clear();
            pilaNumeros.Clear();
            listaVariables.Clear();
            contadorNumeros = 0;
            contadorOperadores = 0;
            lineas = txt_cadena.Text.Split('\n');
            consola.Items.Add("***************************************************");
            if(verificarLlaves()) analizar(0);
            consola.TopIndex = consola.Items.Count - 1;
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void abrirArchivoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "txt files (*.txt)|*.txt";
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Stream file = openFileDialog1.OpenFile();
                StreamReader reader = new StreamReader(file);
                string []nombre = Path.GetFileName(openFileDialog1.FileName).Split('.');
                txt_nombre.Text = nombre[0];
                txt_cadena.Text = reader.ReadToEnd();
                file.Close();
            }
        }

        private void guardarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamWriter file = new StreamWriter(folderBrowserDialog1.SelectedPath + "\\" + txt_nombre.Text + ".txt");
                file.Write(txt_cadena.Text);
                file.Close();
            }
        }

    }
}
