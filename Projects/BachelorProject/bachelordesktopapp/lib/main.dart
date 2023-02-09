import 'dart:convert';
import 'package:bachelordesktopapp/shared_preference_helper.dart';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'connectertype_get.dart';
import 'connectortype_put.dart';
import 'messages.dart';
import 'pod_get.dart';
import 'connector_get.dart';
import 'connector_post.dart';
import 'connector_patch.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatefulWidget  {
  const MyApp({super.key});

  @override
  State<MyApp> createState() => MyAppState();

  static MyAppState of(BuildContext context) => 
      context.findAncestorStateOfType<MyAppState>()!;
}

class MyAppState extends State<MyApp> {
  ThemeMode _themeMode = ThemeMode.system;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Connector Manager',
      theme: ThemeData(),
      darkTheme: ThemeData.dark().copyWith(
        floatingActionButtonTheme: ThemeData.dark().floatingActionButtonTheme.copyWith(
          backgroundColor: Colors.blue,
        ),
      ),
      themeMode: _themeMode,
      home: const MainView(),
    );
  }

  void changeTheme(ThemeMode themeMode) {
    setState(() {
      _themeMode = themeMode;
    });
  }
}

class MainView extends StatefulWidget  {
  const MainView({Key? key}) : super(key: key);

  @override
  State<MainView> createState() => MainViewState();

  static MainViewState of(BuildContext context) => 
      context.findAncestorStateOfType<MainViewState>()!;
}

class MainViewState extends State<MainView> {
  List<String> messages = Messages.messages;
  final List<Widget> _views = const [
    Center(
      child: HomePage(title: 'Home'),
    ),
    Center(
      child: ConnectorsPage(title: 'Connector Overview'),
    ),
    Center(
      child: ConnectorTypesPage(title: 'Connector Types Overview'),
    ),
    Center(
      child: SettingsPage(title: 'Settings Overview'),
    ),
  ];
  int _selectedIndex = 0;
  bool _navigationExtended = true;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      bottomNavigationBar: MediaQuery.of(context).size.width < 500
          ? BottomNavigationBar(
              showUnselectedLabels: true,
              unselectedItemColor: Colors.grey,
              selectedItemColor: Colors.blue,
              currentIndex: _selectedIndex,
              type: BottomNavigationBarType.fixed,
              onTap: (int index) {
                setState(() {
                  _selectedIndex = index;
                });
              },
              items: const [
                BottomNavigationBarItem(
                  icon: Icon(Icons.home_rounded), 
                  label: 'Home'
                ),
                BottomNavigationBarItem(
                  icon: Icon(Icons.device_hub_rounded),
                  label: 'Connectors'
                ),
                BottomNavigationBarItem(
                  icon: Icon(Icons.device_hub_rounded),
                  label: 'Types'
                ),
                BottomNavigationBarItem(
                  icon: Icon(Icons.settings_rounded), 
                  label: 'Settings'
                ),
              ]
            )
          : null,
      body: Row(
        children: [
          if (MediaQuery.of(context).size.width >= 500)
            NavigationRail(
              extended: _navigationExtended,
              destinations: const [
                NavigationRailDestination(
                  icon: Icon(Icons.home_rounded),
                  selectedIcon: Icon(Icons.home_rounded),
                  label: Text('Home'),
                ),
                NavigationRailDestination(
                  icon: Icon(Icons.device_hub_rounded),
                  selectedIcon: Icon(Icons.device_hub_rounded),
                  label: Text('Connectors'),
                ),
                NavigationRailDestination(
                  icon: Icon(Icons.device_hub_rounded),
                  selectedIcon: Icon(Icons.device_hub_rounded),
                  label: Text('Connector Types'),
                ),
                NavigationRailDestination(
                  icon: Icon(Icons.settings),
                  selectedIcon: Icon(Icons.settings_rounded),
                  label: Text('Settings'),
                ),
              ], 
              trailing: Expanded(
                child: Align(
                  alignment: Alignment.bottomCenter,
                  child: IconButton(
                    icon: _navigationExtended ? const Icon(Icons.arrow_left_rounded) : const Icon(Icons.arrow_right_rounded),
                    onPressed: (){
                      setState(() {
                        _navigationExtended ? _navigationExtended = false : _navigationExtended = true;
                      });
                    },
                    splashRadius: 15,
                  ),
                ),
              ),
              onDestinationSelected: (index) {
                setState(() {
                  _selectedIndex = index;
                });
              },
              selectedIndex: _selectedIndex
            ),
          Expanded(
            child: _views.elementAt(_selectedIndex),
          ),
        ],
      ),
    );
  }
}

class HomePage extends StatefulWidget {
  const HomePage({super.key, required this.title});

  final String title;

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  List<ConnectorTypeGet> _connectorTypes = [];
  List<ConnectorGet> _connectors = [];
  ScaffoldMessengerState? messenger;
  bool _suppressNotification = false;

  _showSnackBar(String text) {
    Messages.messages.add(text);
    if(_suppressNotification == false){
      messenger?.showSnackBar(
        SnackBar(
          backgroundColor: const Color.fromARGB(255, 4, 33, 66),
          content: Text(
            text,
            style: const TextStyle(color: Colors.white),
          )
        ),
      );
    }
  }

  Future<void> getData() async {
    debugPrint('Getting Connectors from API');
    try{
      final url = await SharedPreferencesHelper.getAPIURL();
      _suppressNotification = await SharedPreferencesHelper.getSuppressNotification();
      if(url.isEmpty == false){
        final uri = Uri.parse('$url/Connector');
        final response = await http.get(uri);
        
        final uri2 = Uri.parse('$url/Connector/Types');
        final response2 = await http.get(uri2);

        if(response.statusCode >= 200 && response.statusCode < 300 && response2.statusCode >= 200 && response.statusCode < 300){
          final body = response.body;
          final json = jsonDecode(body) as List<dynamic>;
          final transformed = json.map((e) => ConnectorGet.fromJson(e));

          final body2 = response2.body;
          final json2 = jsonDecode(body2) as List<dynamic>;
          final transformed2 = json2.map((e) => ConnectorTypeGet.fromJson(e));

          _connectorTypes = transformed2.toList();
          _connectors = transformed.toList();
        }
        else{
          _showSnackBar('Failed to retrieve data: ${response.reasonPhrase}');
        }
      }
      else{
        _showSnackBar('No API URL found in settings');
      }
    }
    catch(ex)
    {
      _showSnackBar('Problem getting data from API');
    }
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    messenger = ScaffoldMessenger.of(context);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(8.0),
            child: Text('Connector Manager',
              style: Theme.of(context).textTheme.headline2,
            ),
          ),
          const Divider(),
          FutureBuilder(
            builder: (ctx, snapshot) {
              return Expanded(
                child: Column(
                  children: [
                    Expanded(
                      child: Row(
                        children: [
                          Expanded(
                            child: Padding(
                              padding: const EdgeInsets.all(8.0),
                              child: Column(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: [
                                  Text('Connector Types', 
                                    style: Theme.of(context).textTheme.headline5,
                                  ),
                                  Text(_connectorTypes.length.toString(),
                                    style: Theme.of(context).textTheme.headline3,
                                  ),
                                ],
                              ),
                            ),
                          ),
                          const VerticalDivider(),
                          Expanded(
                            child: Padding(
                              padding: const EdgeInsets.all(8.0),
                              child: Column(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: [
                                  Text('Connectors', 
                                    style: Theme.of(context).textTheme.headline5,
                                  ),
                                  Text(_connectors.length.toString(),
                                    style: Theme.of(context).textTheme.headline3,
                                  ),
                                ],
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                    const Divider(),
                    Expanded(
                      child: Padding(
                        padding: const EdgeInsets.all(8.0),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: <Widget>[
                            Text('Messages:',
                              style: Theme.of(context).textTheme.headline6,
                            ),
                            const SizedBox(height: 10),
                            Expanded(
                              child: ListView.builder(
                                itemCount: MainView.of(context).messages.length,
                                itemBuilder: (context, index) {
                                  return Text(MainView.of(context).messages[index]);
                                },
                              ),
                            )
                          ],
                        ),
                      ),
                    ),
                  ],
                ),
              );
            },
            future: getData(),
          )
        ], 
      ),
    );
  }
}

class ConnectorsPage extends StatefulWidget {
  const ConnectorsPage({super.key, required this.title});

  final String title;

  @override
  State<ConnectorsPage> createState() => ConnectorsPageState();

  static ConnectorsPageState of(BuildContext context) => 
      context.findAncestorStateOfType<ConnectorsPageState>()!;
}

class ConnectorsPageState extends State<ConnectorsPage> {
  //List<ConnectorGet> _connectors = [];
  List<ConnectorGet> _connectors = List.generate(20, ((index) {
    var conn = ConnectorGet(
      deploymentName: 'deployment-$index',
      type: 'type',
      image: 'kube-api-latest',
      replicas: 1,
      availableReplicas: 1,
      envVars: List.generate(20, (index2) {return EnvVar(name: 'name-$index2', value: 'value-$index2');})
    );
    return conn;
  }));
  ScaffoldMessengerState? messenger;
  bool _suppressNotification = false;

  _showSnackBar(String text) {
    Messages.messages.add(text);
    if(_suppressNotification == false){
      messenger?.showSnackBar(
        SnackBar(
          backgroundColor: const Color.fromARGB(255, 4, 33, 66),
          content: Text(
            text,
            style: const TextStyle(color: Colors.white),
          )
        ),
      );
    }
  }

  Future<void> getConnectors() async {
    debugPrint('Getting Connectors from API');
    try{
      final url = await SharedPreferencesHelper.getAPIURL();
      _suppressNotification = await SharedPreferencesHelper.getSuppressNotification();
      if(url.isEmpty == false){
        final uri = Uri.parse('$url/Connector');
        final response = await http.get(uri);
        if(response.statusCode >= 200 && response.statusCode < 300){
          final body = response.body;
          final json = jsonDecode(body) as List<dynamic>;
          final transformed = json.map((e) => ConnectorGet.fromJson(e));
          _connectors = transformed.toList();
        }
        else{
          _showSnackBar('Failed to retrieve connectors: ${response.reasonPhrase}');
        }
      }
      else{
        _showSnackBar('No API URL found in settings');
      }
    }
    catch(ex)
    {
      _showSnackBar('Problem getting connectors from API');
    }
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    messenger = ScaffoldMessenger.of(context);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.title),
        backgroundColor: const Color.fromARGB(255, 4, 33, 66),
      ),
      body: FutureBuilder(
        builder: (context, snapshot) {
          if(snapshot.connectionState == ConnectionState.done){
            return Padding(
              padding: const EdgeInsets.all(8.0),
              child: GridView.builder(
                gridDelegate: const SliverGridDelegateWithMaxCrossAxisExtent(maxCrossAxisExtent: 250, childAspectRatio: 1.5, mainAxisExtent: 175),
                itemCount: _connectors.length,
                itemBuilder: (context, i) {
                    return ConnectorCard(connector: _connectors[i],);
                },
              ),
            );
          }
          else{
            return const Center(
              child: CircularProgressIndicator(),
            );
          }
        },
        future: getConnectors(),
      ),
      floatingActionButton: MediaQuery.of(context).size.width >= 500 ? FloatingActionButton.extended(
        onPressed: () {
          showDialog(
            context: context,
            builder: (context) => const PostConnectorDialogPage()
          ).then((value) => setState(() => {}));
        },
        tooltip: 'Add new Connector',
        label: const Text('Add new Connector'),
        icon: const Icon(Icons.add_rounded),
      ) : FloatingActionButton(
        onPressed: () {
          showDialog(
            context: context,
            builder: (context) => const PostConnectorDialogPage()
          ).then((value) => setState(() => {}));
        },
        tooltip: 'Add new Connector',
        child: const Icon(Icons.add_rounded),
      )
    );
  }
}

class ConnectorCard extends StatefulWidget {
  const ConnectorCard({super.key, required this.connector});

  final ConnectorGet connector;

  @override
  State<ConnectorCard> createState() => _ConnectorCardState();
}

class _ConnectorCardState extends State<ConnectorCard> {

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Card(
        clipBehavior: Clip.hardEdge,
        child: InkWell(
          splashColor: Colors.blue.withAlpha(30),
          onTap: () {
            Navigator.of(context).push(
              MaterialPageRoute(
                builder: (context) => ConnectorPage(connector: widget.connector),
                settings: const RouteSettings(name: "/connectors")
              ),
            );
          },
          child: SizedBox(
            width: 225,
            height: 150,        
            child: Column(
              children: [
                Expanded(
                  child: Padding(
                    padding: const EdgeInsets.all(8.0),
                    child: Align(
                      child: Text(widget.connector.deploymentName,
                        style: Theme.of(context).textTheme.headline6,
                        overflow: TextOverflow.ellipsis,
                      ),
                    ),
                  ),
                ),
                const Divider(),
                Expanded(
                  child: Row(
                    children: [
                      Expanded(
                        child: Padding(
                          padding: const EdgeInsets.fromLTRB(8,0,0,0),
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              Text('Image', 
                                style: Theme.of(context).textTheme.labelMedium,
                              ),
                              Text(widget.connector.image,
                                style: Theme.of(context).textTheme.labelSmall,
                                overflow: TextOverflow.ellipsis,
                              ),
                            ],
                          ),
                        )
                      ),
                      const VerticalDivider(),
                      Expanded(
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Text('Replicas', 
                              style: Theme.of(context).textTheme.labelMedium,
                            ),
                            Text('${widget.connector.availableReplicas.toString()}/${widget.connector.replicas.toString()}',
                              style: Theme.of(context).textTheme.headline6,
                            ),
                          ],
                        )
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class ConnectorPage extends StatefulWidget {
  const ConnectorPage({super.key, required this.connector});

  final ConnectorGet connector;

  @override
  State<ConnectorPage> createState() => _ConnectorPageState();
}

class _ConnectorPageState extends State<ConnectorPage> {
  ScaffoldMessengerState? messenger;
  bool _suppressNotification = false;

  List<DataRow> _createRows() {
    return widget.connector.envVars
        .map((envVar) => DataRow(cells: [
              DataCell(Center(child: Text(envVar.name))),
              DataCell(Center(child: Text(envVar.value))),
            ])).toList();
  }

  _showSnackBar(String text) {
    Messages.messages.add(text);
    if(_suppressNotification == false){
      messenger?.showSnackBar(
        SnackBar(
          backgroundColor: const Color.fromARGB(255, 4, 33, 66),
          content: Text(
            text,
            style: const TextStyle(color: Colors.white),
          )
        ),
      );
    }
  }

  Future<void> _deleteConnector() async {
    debugPrint('Sending updated Connector to API');
    try{
      final url = await SharedPreferencesHelper.getAPIURL();
      _suppressNotification = await SharedPreferencesHelper.getSuppressNotification();
      if(url.isEmpty == false){
        final uri = Uri.parse('$url/Connector?deploymentName=${widget.connector.deploymentName}');
        final response = await http.delete(uri);
        if(response.statusCode >= 200 && response.statusCode < 300){
          if (mounted) {
            _showSnackBar('Connector succesfully deleted');
            Navigator.of(context).pop();
          }
        }
        else{
          _showSnackBar('Failed to delete connector: ${response.reasonPhrase}');
        }
      }
      else{
        _showSnackBar('No API URL found in settings');
      }
    }
    catch(ex)
    {
      _showSnackBar('Error while deleting connector');
    }
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    messenger = ScaffoldMessenger.of(context);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.connector.deploymentName),
        backgroundColor: const Color.fromARGB(255, 4, 33, 66),
        actions: [
          IconButton(
            icon: const Icon(Icons.delete_rounded),
            onPressed: () => showDialog(
              context: context,
              builder: (context) => AlertDialog(
                title: const Text('Please Confirm'),
                content: const Text('Are you sure want to delete the connector?'),
                actions: [
                  TextButton(
                    onPressed: () {
                      Navigator.of(context).pop();
                      _deleteConnector();
                    },
                    child: const Text('Yes'),
                  ),
                  TextButton(
                    onPressed: () {
                      Navigator.of(context).pop();
                    },
                    child: const Text('No'),
                  ),
                ],
              )
            ),
          ),
        ],
      ),
      body: Row(
        children: [
          Expanded(
            child: Padding(
              padding: const EdgeInsets.all(20.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Deployment Name:', 
                    style: Theme.of(context).textTheme.headline6
                  ),
                  Text(widget.connector.deploymentName,
                    style: Theme.of(context).textTheme.labelLarge
                  ),
                  const SizedBox(
                    height: 20,
                  ),
                  Text('Type:', 
                    style: Theme.of(context).textTheme.headline6
                  ),
                  Text(widget.connector.type,
                    style: Theme.of(context).textTheme.labelLarge
                  ),
                  const SizedBox(
                    height: 20,
                  ),
                  Text('Image:', 
                    style: Theme.of(context).textTheme.headline6
                  ),
                  Text(widget.connector.image,
                    style: Theme.of(context).textTheme.labelLarge
                  ),
                  const SizedBox(
                    height: 20,
                  ),
                  Text('Replicas:', 
                    style: Theme.of(context).textTheme.headline6
                  ),
                  Text('${widget.connector.availableReplicas}/${widget.connector.replicas}',
                    style: Theme.of(context).textTheme.labelLarge
                  ),
                  const SizedBox(
                    height: 20,
                  ),
                  if (MediaQuery.of(context).size.width < 500 && widget.connector.envVars.isNotEmpty) SizedBox(
                    height: MediaQuery.of(context).size.height - 460,
                    child: ListView(
                      children: [
                        DataTable(
                          columns: [
                            DataColumn(label: Expanded(child: Text('Name', textAlign: TextAlign.center, style: Theme.of(context).textTheme.headline6,))),
                            DataColumn(label: Expanded(child: Text('Value', textAlign: TextAlign.center, style: Theme.of(context).textTheme.headline6,)))
                          ], 
                          rows: _createRows(),
                          showBottomBorder: true,
                        ),
                      ],
                    ),
                  ) else Container(),
                  Expanded(
                    child: Align(
                      alignment: Alignment.bottomCenter,
                      child: TextButton.icon(
                        icon: const Icon(Icons.description_rounded,size: 36,),
                        label: const Text('See Logs',),
                        onPressed: () => {
                          Navigator.of(context).push(
                            MaterialPageRoute(
                              builder: (context) => PodPage(connectorName: widget.connector.deploymentName,),
                            ),
                          ),
                        },
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
          if (MediaQuery.of(context).size.width >= 500 && widget.connector.envVars.isNotEmpty) Expanded(child:  
            Padding(
              padding: const EdgeInsets.all(8.0),
              child: Align(
                alignment: Alignment.topCenter,
                child: 
                SizedBox(
                  width: 400,
                  height: MediaQuery.of(context).size.height - 150,
                  child: ListView(
                    children: [
                      DataTable(
                        
                        columns: [
                        DataColumn(label: Expanded(child: Text('Name', textAlign: TextAlign.center, style: Theme.of(context).textTheme.headline6,))),
                        DataColumn(label: Expanded(child: Text('Value', textAlign: TextAlign.center, style: Theme.of(context).textTheme.headline6,)))
                      ], 
                      rows: _createRows())
                    ],
                  ),
                ),
              ),
            ),
          ) else Container()
        ],
      ),
      floatingActionButton: MediaQuery.of(context).size.width >= 500 ? FloatingActionButton.extended(
        onPressed: () {
          showDialog(
            context: context,
            builder: (context) => PatchConnectorDialogPage(connector: widget.connector)
          );
        },
        tooltip: 'Edit Connector',
        label: const Text('Edit Connector'),
        icon: const Icon(Icons.edit_rounded),
      ) : FloatingActionButton(
        onPressed: () {
          showDialog(
            context: context,
            builder: (context) => PatchConnectorDialogPage(connector: widget.connector)
          );
        },
        tooltip: 'Edit Connector',
        child: const Icon(Icons.edit_rounded),
      )
    );
  }
}

class PodPage extends StatefulWidget {
  const PodPage({super.key, required this.connectorName});

  final String connectorName;

  @override
  State<PodPage> createState() => _PodPageState();
}

class _PodPageState extends State<PodPage> {
  List<PodGet> _pods = List.generate(4, ((index) => PodGet(name: 'pod $index')));
  ScaffoldMessengerState? messenger;
  bool _suppressNotification = false;

  _showSnackBar(String text) {
    Messages.messages.add(text);
    if(_suppressNotification == false){
      messenger?.showSnackBar(
        SnackBar(
          backgroundColor: const Color.fromARGB(255, 4, 33, 66),
          content: Text(
            text,
            style: const TextStyle(color: Colors.white),
          ) 
        ),
      );
    }
  }

  Future<void> getPods() async {
    debugPrint('Getting pods from API');
    try{
      final url = await SharedPreferencesHelper.getAPIURL();
      _suppressNotification = await SharedPreferencesHelper.getSuppressNotification();
      if(url.isEmpty == false){
        final uri = Uri.parse('$url/Connector/${widget.connectorName}/Pods');
        final response = await http.get(uri);
        if(response.statusCode >= 200 && response.statusCode < 300){
          final body = response.body;
          final json = jsonDecode(body) as List<dynamic>;
          final transformed = json.map((e) => PodGet.fromJson(e));
          _pods = transformed.toList();
        }
        else{
          _showSnackBar('Failed to retrieve pods: ${response.reasonPhrase}');
        }
      }
      else{
        _showSnackBar('No API URL found in settings');
      }
    }
    catch(ex)
    {
      _showSnackBar('Problem getting pods from API');
    }
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    messenger = ScaffoldMessenger.of(context);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('${widget.connectorName} Pods'),
        backgroundColor: const Color.fromARGB(255, 4, 33, 66),
      ),
      body:  Padding(
        padding: const EdgeInsets.fromLTRB(8,20,8,8),
        child: FutureBuilder(
          builder: (ctx, snapshot) {
            if(snapshot.connectionState == ConnectionState.done){
              return GridView.builder(
                gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(crossAxisCount: 3, mainAxisExtent: 150),
                itemCount: _pods.length,
                itemBuilder: (ctx, i) {
                  return Center(
                    child: Column(
                      children: [
                        Card(
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(100.0),
                          ),
                          clipBehavior: Clip.hardEdge,
                          child: InkWell(
                            splashColor: Colors.blue.withAlpha(30),
                            onTap: () {
                              Navigator.of(context).push(
                                MaterialPageRoute(
                                  builder: (context) => LogPage(podName: _pods[i].name,),
                                ),
                              );
                            },
                            child: const Padding(
                              padding: EdgeInsets.all(8.0),
                              child: Icon(Icons.description_rounded, size: 60),
                            ),
                          ),
                        ),
                        Padding(
                          padding: const EdgeInsets.all(8.0),
                          child: Text(_pods[i].name,
                            style: Theme.of(context).textTheme.labelMedium,
                          ),
                        ),
                      ],
                    ),
                  );
                },
              );
            }
            else
            {
              return const Center(
                child: CircularProgressIndicator(),
              );
            }
          },
          future: getPods(),
        ),
      ),
    );
  }
}

class LogPage extends StatefulWidget {
  const LogPage({super.key, required this.podName});

  final String podName;

  @override
  State<LogPage> createState() => _LogPageState();
}

class _LogPageState extends State<LogPage> {
  String log = 'Loading...';
  ScaffoldMessengerState? messenger;
  bool _suppressNotification = false;

  _showSnackBar(String text) {
    Messages.messages.add(text);
    if(_suppressNotification == false){
      messenger?.showSnackBar(
        SnackBar(
          backgroundColor: const Color.fromARGB(255, 4, 33, 66),
          content: Text(
            text,
            style: const TextStyle(color: Colors.white),
          )
        ),
      );
    }
  }

  Future<void> getLog() async {
    debugPrint('Getting log from API');
    try{
      final url = await SharedPreferencesHelper.getAPIURL();
      _suppressNotification = await SharedPreferencesHelper.getSuppressNotification();
      if(url.isEmpty == false){
        final uri = Uri.parse('$url/Pod/${widget.podName}/Logs');
        final response = await http.get(uri);
        if(response.statusCode >= 200 && response.statusCode < 300){
          final body = response.body;
          log = body;
        }
        else{
          _showSnackBar('Failed to retrieve log: ${response.reasonPhrase}');
        }
      }
      else{
        _showSnackBar('No API URL found in settings');
      }
      
    }
    catch(ex)
    {
      _showSnackBar('Problem getting log from API');
    }
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    messenger = ScaffoldMessenger.of(context);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.podName),
        backgroundColor: const Color.fromARGB(255, 4, 33, 66),
      ),
      body: FutureBuilder(
        builder: (ctx, snapshot) {
          if(snapshot.connectionState == ConnectionState.done){
            return CustomScrollView(
              slivers: [
                SliverFillRemaining(
                  hasScrollBody: false,
                  child: Padding(
                    padding: const EdgeInsets.all(8.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: <Widget>[
                        Expanded(child: Text(log),),
                      ],
                    ),
                  ),
                ),
              ],
            );
          }
          else
          {
            return const Center(
              child: CircularProgressIndicator(),
            );
          }
        },
        future: getLog(),
      ),
    );
  }
}

class ConnectorTypesPage extends StatefulWidget {
  const ConnectorTypesPage({super.key, required this.title});

  final String title;

  @override
  State<ConnectorTypesPage> createState() => ConnectorTypesPageState();
}

class ConnectorTypesPageState extends State<ConnectorTypesPage> {
  //List<ConnectorTypeGet> _connectorTypes = [];
  List<ConnectorTypeGet> _connectorTypes = List.generate(20, ((index) {
    var conn = ConnectorTypeGet(
      type: 'type',
      repository: 'kube-api-repository',
      maxReplicas: 1,
      exposedPorts: List.generate(20, (index2) {return index2;}),
      images: List.generate(20, (index2) {return index2.toString();})
    );
    return conn;
  }));
  ScaffoldMessengerState? messenger;
  bool _suppressNotification = false;

  _showSnackBar(String text) {
    Messages.messages.add(text);
    if(_suppressNotification == false){
      messenger?.showSnackBar(
        SnackBar(
          backgroundColor: const Color.fromARGB(255, 4, 33, 66),
          content: Text(
            text,
            style: const TextStyle(color: Colors.white),
          ) 
        ),
      );
    }
  }

  Future<void> getConnectorTypes() async {
    debugPrint('Getting ConnectorTypes from API');
    try{
      final url = await SharedPreferencesHelper.getAPIURL();
      _suppressNotification = await SharedPreferencesHelper.getSuppressNotification();
      if(url.isEmpty == false){
        final uri = Uri.parse('$url/Connector/Types');
        final response = await http.get(uri);
        if(response.statusCode >= 200 && response.statusCode < 300){
          final body = response.body;
          final json = jsonDecode(body) as List<dynamic>;
          final transformed = json.map((e) => ConnectorTypeGet.fromJson(e));
          _connectorTypes = transformed.toList();
        }
        else{
          _showSnackBar('Failed to retrieve connector types: ${response.reasonPhrase}');
        }
      }
      else{
        _showSnackBar('No API URL found in settings');
      }
    }
    catch(ex)
    {
      _showSnackBar('Problem getting connectorTypes from API');
    }
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    messenger = ScaffoldMessenger.of(context);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.title),
        backgroundColor: const Color.fromARGB(255, 4, 33, 66),
      ),
      body: FutureBuilder(
        builder: (ctx, snapshot) {
          if(snapshot.connectionState == ConnectionState.done){
            return Padding(
              padding: const EdgeInsets.all(8.0),
              child: GridView.builder(
                gridDelegate: const SliverGridDelegateWithMaxCrossAxisExtent(maxCrossAxisExtent: 250, childAspectRatio: 1.5, mainAxisExtent: 175),
                itemCount: _connectorTypes.length,
                itemBuilder: (ctx, i) {
                    return ConnectorTypeCard(connectorType: _connectorTypes[i],);
                },
              ),
            );
          }
          else{
            return const Center(
              child: CircularProgressIndicator(),
            );
          }
        },
        future: getConnectorTypes(),
      ),
    );
  }
}

class ConnectorTypeCard extends StatefulWidget {
  const ConnectorTypeCard({super.key, required this.connectorType});

  final ConnectorTypeGet connectorType;

  @override
  State<ConnectorTypeCard> createState() => _ConnectorTypeCardState();
}

class _ConnectorTypeCardState extends State<ConnectorTypeCard> {

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Card(
        clipBehavior: Clip.hardEdge,
        child: InkWell(
          splashColor: Colors.blue.withAlpha(30),
          onTap: () {
            Navigator.of(context).push(
              MaterialPageRoute(
                builder: (context) => ConnectorTypePage(connectorType: widget.connectorType)
              ),
            );
          },
          child: SizedBox(
            width: 225,
            height: 150,        
            child: Column(
              children: [
                Expanded(
                  child: Padding(
                    padding: const EdgeInsets.all(8.0),
                    child: Align(
                      child: Text(widget.connectorType.type,
                        style: Theme.of(context).textTheme.headline6,
                        overflow: TextOverflow.ellipsis,
                      ),
                    ),
                  ),
                ),
                const Divider(),
                Expanded(
                  child: Row(
                    children: [
                      Expanded(
                        child: Padding(
                          padding: const EdgeInsets.fromLTRB(8,0,0,0),
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              Text('Exposed Port', 
                                style: Theme.of(context).textTheme.labelMedium,
                              ),
                              Text(widget.connectorType.exposedPorts.isNotEmpty ? widget.connectorType.exposedPorts.first.toString() : 'X',
                                style: Theme.of(context).textTheme.headline6,
                                overflow: TextOverflow.ellipsis,
                              ),
                            ],
                          ),
                        ),
                      ),
                      const VerticalDivider(),
                      Expanded(
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Text('Max Replicas', 
                              style: Theme.of(context).textTheme.labelMedium,
                            ),
                            Text(widget.connectorType.maxReplicas.toString(),
                              style: Theme.of(context).textTheme.headline6,
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class ConnectorTypePage extends StatefulWidget {
  const ConnectorTypePage({super.key, required this.connectorType});

  final ConnectorTypeGet connectorType;

  @override
  State<ConnectorTypePage> createState() => _ConnectorTypePageState();
}

class _ConnectorTypePageState extends State<ConnectorTypePage> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.connectorType.type),
        backgroundColor: const Color.fromARGB(255, 4, 33, 66),
      ),
      body: Row(
        children: [
          Expanded(
            child: Padding(
              padding: const EdgeInsets.all(20.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Type:', 
                    style: Theme.of(context).textTheme.headline6
                  ),
                  Text(widget.connectorType.type,
                    style: Theme.of(context).textTheme.labelLarge
                  ),
                  const SizedBox(
                    height: 20,
                  ),
                  Text('Repository:', 
                    style: Theme.of(context).textTheme.headline6
                  ),
                  Text(widget.connectorType.repository,
                    style: Theme.of(context).textTheme.labelLarge
                  ),
                  const SizedBox(
                    height: 20,
                  ),
                  Text('Exposed Ports:', 
                    style: Theme.of(context).textTheme.headline6
                  ),
                  Text(widget.connectorType.exposedPorts.isNotEmpty ? widget.connectorType.exposedPorts.join(', ') : 'All Closed',
                    style: Theme.of(context).textTheme.labelLarge
                  ),
                  const SizedBox(
                    height: 20,
                  ),
                  Text('Available Images:', 
                    style: Theme.of(context).textTheme.headline6
                  ),
                  Text(widget.connectorType.images.isNotEmpty ? widget.connectorType.images.join(', ') : 'No Images',
                    style: Theme.of(context).textTheme.labelLarge
                  ),
                  const SizedBox(
                    height: 20,
                  ),
                  Text('Max Replicas:', 
                    style: Theme.of(context).textTheme.headline6
                  ),
                  Text(widget.connectorType.maxReplicas.toString(),
                    style: Theme.of(context).textTheme.labelLarge
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
      floatingActionButton: MediaQuery.of(context).size.width >= 500 ? FloatingActionButton.extended(
        onPressed: () {
          showDialog(
            context: context,
            builder: (context) => PutConnectorTypeDialogPage(connectorType: widget.connectorType)
          );
        },
        tooltip: 'Edit Connector Type',
        label: const Text('Edit Connector Type'),
        icon: const Icon(Icons.edit_rounded),
      ) : FloatingActionButton(
        onPressed: () {
          showDialog(
            context: context,
            builder: (context) => PutConnectorTypeDialogPage(connectorType: widget.connectorType)
          );
        },
        tooltip: 'Edit Connector Type',
        child: const Icon(Icons.edit_rounded),
      )
    );
  }
}

class SettingsPage extends StatefulWidget {
  const SettingsPage({super.key, required this.title});

  final String title;

  @override
  State<SettingsPage> createState() => _SettingsPageState();
}

class _SettingsPageState extends State<SettingsPage> {
  final _textController = TextEditingController();
  bool _suppressNotification = false;

  Future<void> getSettings() async {
    debugPrint('agsikgsmg');
    _textController.text = await SharedPreferencesHelper.getAPIURL();
    _suppressNotification = await SharedPreferencesHelper.getSuppressNotification();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.title),
        backgroundColor: const Color.fromARGB(255, 4, 33, 66),
      ),
      body: Padding(
        padding: const EdgeInsets.fromLTRB(20,50,20,0),
        child: FutureBuilder(
          builder: (ctx, snapshot) {
            if(snapshot.connectionState == ConnectionState.done){
              return Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisAlignment: MainAxisAlignment.start,
                children: <Widget>[
                  SizedBox(
                    width: 400.0,
                    child: TextField(
                      controller: _textController,
                      decoration: InputDecoration(
                        labelText: 'API URL',
                        labelStyle: const TextStyle(
                          fontSize: 25,
                        ),
                        hintText: "https://apiurl:portnumber",
                        border: const OutlineInputBorder(),
                        suffixIcon: IconButton(
                          onPressed: () {
                            _textController.clear();
                          }, 
                          icon: const Icon(Icons.clear_rounded),
                          splashRadius: 15,
                          )
                      ),
                      onChanged: (value) {
                        SharedPreferencesHelper.setAPIURL(value);
                      },
                    ),
                  ),
                  const SizedBox(
                    height: 20,
                  ),
                  SizedBox(
                    width: 400.0,
                    child: SwitchListTile(
                      title: Text('Dark Mode',style: Theme.of(context).textTheme.labelLarge,),
                      value: Theme.of(context).brightness == Brightness.dark,
                      activeColor: Colors.blue,
                      onChanged: (bool val) =>
                        setState(() {
                          val ? MyApp.of(context).changeTheme(ThemeMode.dark) : MyApp.of(context).changeTheme(ThemeMode.light);
                        } 
                      ),
                    ),
                  ),
                  const SizedBox(
                    height: 20,
                  ),
                  SizedBox(
                    width: 400.0,
                    child: SwitchListTile(
                      title: Text('Suppress Notifications',style: Theme.of(context).textTheme.labelLarge,),
                      value: _suppressNotification,
                      activeColor: Colors.blue,
                      onChanged: (bool val) {
                        SharedPreferencesHelper.setSuppressNotification(val);
                        setState(() {
                          _suppressNotification = val;
                        });
                      }
                    ),
                  ),
                ],
              );
            }
            else
            {
              return const Center(
                child: CircularProgressIndicator(),
              );
            }
          },
          future: getSettings(),
        ),
      ),
    );
  }
}


class PostConnectorDialogPage extends StatefulWidget {
  const PostConnectorDialogPage({super.key});

  @override
  State<PostConnectorDialogPage> createState() => _PostConnectorDialogPageState();
}

class _PostConnectorDialogPageState extends State<PostConnectorDialogPage> {
  final _formKey = GlobalKey<FormState>();
  final _connectorPost = ConnectorPost();
  ScaffoldMessengerState? messenger;
  bool _suppressNotification = false;

  _showSnackBar(String text) {
    Messages.messages.add(text);
    if(_suppressNotification == false){
      messenger?.showSnackBar(
        SnackBar(
          backgroundColor: const Color.fromARGB(255, 4, 33, 66),
          content: Text(
            text,
            style: const TextStyle(color: Colors.white),
          )
        ),
      );
    }
  }

  Future<void> _postConnector() async {
    debugPrint('Sending new Connector to API');
    try{
      final url = await SharedPreferencesHelper.getAPIURL();
      _suppressNotification = await SharedPreferencesHelper.getSuppressNotification();
      _showSnackBar('Submitting form to API');
      if(url.isEmpty == false){
        final uri = Uri.parse('$url/Connector');
        final response = await http.post(uri,
          headers: {'Content-Type': 'application/json'},
          body: jsonEncode(_connectorPost.toJson()),
        );
        if(response.statusCode >= 200 && response.statusCode < 300){
          _showSnackBar('Connector succesfully created');
        }
        else{
          _showSnackBar('Failed to create connector: ${response.reasonPhrase}');
        }
      }
      else{
        _showSnackBar('No API URL found in settings');
      }
    }
    catch(ex)
    {
      _showSnackBar('Error while creating connector');
    }
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    messenger = ScaffoldMessenger.of(context);
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      content: Stack(
        children: <Widget>[
          Positioned(
            right: -40.0,
            top: -40.0,
            child: InkResponse(
              onTap: () {
                Navigator.of(context).pop();
              },
              child: const CircleAvatar(
                backgroundColor: Colors.red,
                child: Icon(Icons.close_rounded),
              ),
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(8.0),
            child: Form(
              key: _formKey,
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: <Widget>[
                  TextFormField(
                    decoration: const InputDecoration(
                      labelText: 'Deployment Name',
                      icon: Icon(Icons.hub_rounded),
                    ),
                    validator: (value) {
                      if (value?.isEmpty == true) {
                        return 'Please enter a deployment name';
                      }
                      return null;
                    },
                    onSaved: (val) =>
                      setState(() => _connectorPost.deploymentName = val ?? ''),
                  ),
                  const SizedBox(height: 15),
                  TextFormField(
                    decoration: const InputDecoration(
                      labelText: 'Type',
                      icon: Icon(Icons.category_rounded),
                    ),
                    validator: (value) {
                      if (value?.isEmpty == true) {
                        return 'Please enter a connector type';
                      }
                      return null;
                    },
                    onSaved: (val) =>
                      setState(() => _connectorPost.type = val ?? ''),
                  ),
                  const SizedBox(height: 15),
                  TextFormField(
                    decoration: const InputDecoration(
                      labelText: 'Image',
                      icon: Icon(Icons.camera_rounded),
                    ),
                    validator: (value) {
                      if (value?.isEmpty == true) {
                        return 'Please enter an image';
                      }
                      return null;
                    },
                    onSaved: (val) =>
                      setState(() => _connectorPost.image = val ?? ''),
                  ),
                  const SizedBox(height: 15),
                  TextFormField(
                    decoration: const InputDecoration(
                      labelText: 'Replicas',
                      icon: Icon(Icons.group_work_rounded),
                    ),
                    validator: (value) {
                      if (value?.isEmpty == true) {
                        return 'Please enter a replica amount';
                      }
                      return null;
                    },
                    onSaved: (val) =>
                      setState(() => _connectorPost.replicas = val == null ? 0 : int.tryParse(val) ?? 0),
                  ),
                  const SizedBox(height: 20),
                  const Divider(),
                  SwitchListTile(
                    title: const Text('Create Service'),
                    value: _connectorPost.createService,
                    activeColor: Colors.blue,
                    onChanged: (bool val) =>
                        setState(() => _connectorPost.createService = val)),
                  const SizedBox(height: 10),
                  TextFormField(                    
                    enabled: _connectorPost.createService,          
                    decoration: const InputDecoration(
                      labelText: 'External Port Number',
                      icon: Icon(Icons.dns_rounded),
                      errorMaxLines: 2
                    ),
                    validator: (value) {
                      if(!_connectorPost.createService){
                        return null;
                      }
                      if (value?.isEmpty == true || value == null) {
                        return 'Please enter an external port number';
                      }
                      if((int.tryParse(value) ?? 0) < 30000){
                        return 'Port number has to be above 30000';
                      }
                      return null;
                    },
                    onSaved: (val) =>
                      setState(() => _connectorPost.externalPortNumber = val == null ? 0 : int.tryParse(val) ?? 0),
                  ),
                  const SizedBox(height: 15),
                  const Divider(),
                  Padding(
                    padding: const EdgeInsets.all(8.0),
                    child: ElevatedButton(
                      child: const Text('Submit'),
                      onPressed: () {
                        if (_formKey.currentState?.validate() == true) {
                          _formKey.currentState?.save();
                          _postConnector();
                          Navigator.of(context).pop();
                        }
                      },
                    ),
                  ),
                  const Divider(),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class PatchConnectorDialogPage extends StatefulWidget {
  const PatchConnectorDialogPage({super.key, required this.connector});

  final ConnectorGet connector;

  @override
  State<PatchConnectorDialogPage> createState() => _PatchConnectorDialogPageState();
}

class _PatchConnectorDialogPageState extends State<PatchConnectorDialogPage> {
  final _formKey = GlobalKey<FormState>();
  final _connectorPatch = ConnectorPatch();
  ScaffoldMessengerState? messenger;
  bool _suppressNotification = false;

  _showSnackBar(String text) {
    Messages.messages.add(text);
    if(_suppressNotification == false){
      messenger?.showSnackBar(
        SnackBar(
          backgroundColor: const Color.fromARGB(255, 4, 33, 66),
          content: Text(
            text,
            style: const TextStyle(color: Colors.white),
          ) 
        ),
      );
    }
  }

  Future<void> _patchConnector() async {
    debugPrint('Sending updated Connector to API');
    try{
      final url = await SharedPreferencesHelper.getAPIURL();
      _suppressNotification = await SharedPreferencesHelper.getSuppressNotification();
      _showSnackBar('Submitting form to API');
      if(url.isEmpty == false){
        final uri = Uri.parse('$url/Connector?deploymentName=${widget.connector.deploymentName}');
        final response = await http.patch(uri,
          headers: {'Content-Type': 'application/json'},
          body: jsonEncode(_connectorPatch.toJson()),
        );
        if(response.statusCode >= 200 && response.statusCode < 300){
          _showSnackBar('Connector succesfully updated');
        }
        else{
          _showSnackBar('Failed to update connector: ${response.reasonPhrase}');
        }
      }
      else{
        _showSnackBar('No API URL found in settings');
      }
    }
    catch(ex)
    {
      _showSnackBar('Error while updating connector');
    }
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    messenger = ScaffoldMessenger.of(context);
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      content: Stack(
        children: <Widget>[
          Positioned(
            right: -40.0,
            top: -40.0,
            child: InkResponse(
              onTap: () {
                Navigator.of(context).pop();
              },
              child: const CircleAvatar(
                backgroundColor: Colors.red,
                child: Icon(Icons.close_rounded),
              ),
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(8.0),
            child: Form(
              key: _formKey,
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: <Widget>[
                  TextFormField(
                    enabled: false,
                    initialValue: widget.connector.deploymentName,
                    decoration: const InputDecoration(
                      labelText: 'Deployment Name',
                      icon: Icon(Icons.hub_rounded),
                    ),
                    onSaved: (val) =>
                      setState(() => _connectorPatch.deploymentName = val ?? ''),
                  ),
                  const SizedBox(height: 15),
                  TextFormField(
                    enabled: false,
                    initialValue: widget.connector.type,
                    decoration: const InputDecoration(
                      labelText: 'Type',
                      icon: Icon(Icons.category_rounded),
                    ),
                    onSaved: (val) =>
                      setState(() => _connectorPatch.type = val ?? ''),
                  ),
                  const SizedBox(height: 15),
                  TextFormField(
                    decoration: const InputDecoration(
                      labelText: 'Image Tag',
                      icon: Icon(Icons.camera_rounded),
                    ),
                    validator: (value) {
                      if (value?.isEmpty == true) {
                        return 'Please enter an image';
                      }
                      return null;
                    },
                    onSaved: (val) =>
                      setState(() => _connectorPatch.image = val ?? ''),
                  ),
                  const SizedBox(height: 15),
                  TextFormField(
                    decoration: const InputDecoration(
                      labelText: 'Replicas',
                      icon: Icon(Icons.dns_rounded),
                    ),
                    validator: (value) {
                      if (value?.isEmpty == true) {
                        return 'Please enter a replica amount';
                      }
                      return null;
                    },
                    onSaved: (val) =>
                      setState(() => _connectorPatch.replicas = val == null ? 0 : int.tryParse(val) ?? 0),
                  ),
                  const SizedBox(height: 15),
                  const Divider(),
                  Padding(
                    padding: const EdgeInsets.all(8.0),
                    child: ElevatedButton(
                      child: const Text('Submit'),
                      onPressed: () {
                        if (_formKey.currentState?.validate() == true) {
                          _formKey.currentState?.save();
                          _patchConnector();
                          Navigator.of(context).pop();
                        }
                      },
                    ),
                  ),
                  const Divider(),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class PutConnectorTypeDialogPage extends StatefulWidget {
  const PutConnectorTypeDialogPage({super.key, required this.connectorType});

  final ConnectorTypeGet connectorType;

  @override
  State<PutConnectorTypeDialogPage> createState() => _PutConnectorTypeDialogPageState();
}

class _PutConnectorTypeDialogPageState extends State<PutConnectorTypeDialogPage> {
  final _formKey = GlobalKey<FormState>();
  final _connectorTypePut = ConnectorTypePut();
  ScaffoldMessengerState? messenger;
  bool _suppressNotification = false;

  _showSnackBar(String text) {
    Messages.messages.add(text);
    if(_suppressNotification == false){
      messenger?.showSnackBar(
        SnackBar(
          backgroundColor: const Color.fromARGB(255, 4, 33, 66),     
          content: Text(
            text,
            style: const TextStyle(color: Colors.white),
          )
        ),
      );
    }
  }

  Future<void> putConnectorType() async {
    debugPrint('Sending updated Connector Type to API');
    try{
      final url = await SharedPreferencesHelper.getAPIURL();
      _suppressNotification = await SharedPreferencesHelper.getSuppressNotification();
      _showSnackBar('Submitting form to API');
      if(url.isEmpty == false){
        final uri = Uri.parse('$url/Connector/Types?type=${_connectorTypePut.type}');
        final response = await http.put(uri,
          headers: {'Content-Type': 'application/json'},
          body: jsonEncode(_connectorTypePut.toJson()),
        );
        if(response.statusCode >= 200 && response.statusCode < 300){
          _showSnackBar('Connector type succesfully updated');
        }
        else{
          _showSnackBar('Connector type failed to update: ${response.reasonPhrase}');
        }
      }
      else{
        _showSnackBar('No API URL found in settings');
      }
    }
    catch(ex)
    {
      _showSnackBar('Error while updating connector type');
    }
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    messenger = ScaffoldMessenger.of(context);
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      content: Stack(
        children: <Widget>[
          Positioned(
            right: -40.0,
            top: -40.0,
            child: InkResponse(
              onTap: () {
                Navigator.of(context).pop();
              },
              child: const CircleAvatar(
                backgroundColor: Colors.red,
                child: Icon(Icons.close_rounded),
              ),
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(8.0),
            child: Form(
              key: _formKey,
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: <Widget>[
                  TextFormField(
                    enabled: false,
                    initialValue: widget.connectorType.type,
                    decoration: const InputDecoration(
                      labelText: 'Type',
                      icon: Icon(Icons.category_rounded),
                    ),
                    onSaved: (val) =>
                      setState(() => _connectorTypePut.type = val ?? ''),
                  ),
                  const SizedBox(height: 15),
                  TextFormField(
                    initialValue: widget.connectorType.repository,
                    decoration: const InputDecoration(
                      labelText: 'Repository',
                      icon: Icon(Icons.cloud_rounded),
                    ),
                    onSaved: (val) =>
                      setState(() => _connectorTypePut.repository = val ?? ''),
                  ),
                  const SizedBox(height: 15),
                  TextFormField(
                    decoration: const InputDecoration(
                      labelText: 'Exposed Port',
                      icon: Icon(Icons.dns_rounded),
                    ),
                    validator: (value) {
                      if (value?.isEmpty == true) {
                        return 'Please enter an exposed port';
                      }
                      return null;
                    },
                    onSaved: (val) =>
                      setState(() => (val != null && int.tryParse(val) != null) ? _connectorTypePut.exposedPorts.add(int.tryParse(val) ?? 0) : 0),
                  ),
                  TextFormField(
                    initialValue: widget.connectorType.maxReplicas.toString(),
                    decoration: const InputDecoration(
                      labelText: 'Max Replicas',
                      icon: Icon(Icons.group_work_rounded),
                    ),
                    validator: (value) {
                      if (value != null && value.isEmpty == false && int.tryParse(value) == null) {
                        return 'Please enter a number or leave blank for no limit';
                      }
                      return null;
                    },
                    onSaved: (val) =>
                      setState(() => _connectorTypePut.maxReplicas = val == null ? null : int.tryParse(val)),
                  ),
                  const SizedBox(height: 15),
                  const Divider(),
                  Padding(
                    padding: const EdgeInsets.all(8.0),
                    child: ElevatedButton(
                      child: const Text('Submit'),
                      onPressed: () {
                        if (_formKey.currentState?.validate() == true) {
                          _formKey.currentState?.save();
                          putConnectorType();
                          Navigator.of(context).pop();
                        }
                      },
                    ),
                  ),
                  const Divider(),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}