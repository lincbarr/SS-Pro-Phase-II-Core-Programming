using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header ("Enemy Types")]
    [SerializeField] private GameObject _bossEnemyPrefab;
    [SerializeField] private GameObject[] _enemyPrefabs;
    [SerializeField] private GameObject _enemyContainer;
    [Header ("PowerUps")]
    //[SerializeField] private GameObject _minePowerUp;
    [SerializeField] private GameObject[] _powerUps;

    //Random Distribution
    private RandomDistribution _randomDistribution;

    //Wave System
    private int _maxNumberOfWaves = 10;
    private int _currentWave = 1;
    private int _addEnemiesEachWave = 2;
    private float _waveInterval = 5f;
    private int _numberOfEnemiesInThisWave = 1;

    // Move on angle
    private bool _moveOnAngle = false;
    private float _minAngle = -30f;  // In degrees
    private float _maxAngle = 30f;  // In degrees

    private void Start()
    {
        _currentWave = 1;
        _randomDistribution = GetComponent<RandomDistribution>();
        if (_randomDistribution == null)
        {
            Debug.LogError("SpawnManager: Unable to find RandomDistribution script.");
        }
    }

    public void StartSpawning()
    {
        //StartCoroutine(SpawnEnemyRoutine());
        StartCoroutine(SpawnEnemyWaves());
        StartCoroutine(SpawnPowerUpRoutine());
        //StartCoroutine(SpawnSpaceMineRoutine());
    }

    private IEnumerator SpawnEnemyRoutine()
    {
        yield return new WaitForSeconds(2.0f);

        while(true)
        {
            int enemyIdx = Random.Range(0, _enemyPrefabs.Length);
            float randomX = Random.Range(-8.0f, 8.0f);
            Vector3 posToSpawn = new Vector3(randomX, 7.5f, 0f);
            GameObject newEnemy = Instantiate(_enemyPrefabs[enemyIdx], posToSpawn, Quaternion.identity);
            newEnemy.transform.parent = _enemyContainer.transform;
            yield return new WaitForSeconds(5.0f);
        }
    }

    private IEnumerator SpawnEnemyWaves()
    {
        bool _run = true;
        int _bossEnemyNumber = 1;
        bool _createBoss = false;
        GameObject _newEnemy;
        Quaternion _rotation;

        yield return new WaitForSeconds(2.0f);

        while(_run)
        {
            while(_currentWave <= _maxNumberOfWaves)
            {
                int waveType = Random.Range(0, 2);

                _moveOnAngle = waveType == 0 ? false : true;
                
                if (_currentWave >= _maxNumberOfWaves)
                {
                    _bossEnemyNumber = Random.Range(1, _numberOfEnemiesInThisWave);
                    _createBoss = true;
                }

                if (_moveOnAngle == true)
                {
                    float angle = Random.Range(_minAngle, _maxAngle);

                    _rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                } else
                {
                    _rotation = Quaternion.AngleAxis(0f, Vector3.forward);
                }


                for (int i = 1; i <= _numberOfEnemiesInThisWave; i++)
                {
                    int enemyIdx = Random.Range(0, _enemyPrefabs.Length);

                    float randomX = Random.Range(-8.0f, 8.0f);
                    Vector3 posToSpawn = new Vector3(randomX, 7.5f, 0f);
                    if (_createBoss == true && i == _bossEnemyNumber)
                    {
                        _newEnemy = Instantiate(_bossEnemyPrefab, new Vector3(0.0f, 7.5f, 0.0f), Quaternion.identity);
                    }
                    else
                    {
                        _newEnemy = Instantiate(_enemyPrefabs[enemyIdx], posToSpawn, Quaternion.identity);
                        
                    }
                    if (_moveOnAngle == true)
                    {
                        _newEnemy.transform.rotation = _rotation;
                    }
                    _newEnemy.transform.parent = _enemyContainer.transform;
                    yield return new WaitForSeconds(0.5f);
                }
                
                _numberOfEnemiesInThisWave += _addEnemiesEachWave;
                _currentWave++;
                yield return new WaitForSeconds(_waveInterval);
            }

            _run = false;
            
        }


    } 


    private IEnumerator SpawnPowerUpRoutine()
    {
        yield return new WaitForSeconds(3.0f);

        while (true)
        {
            int randomPowerUp = _randomDistribution.RandomInt();  //Random.Range(0, _powerUps.Length);
            float randomSeconds = Random.Range(3f, 8f);
            float randomX = Random.Range(-8.0f, 8.0f);
            Vector3 posToSpawn = new Vector3(randomX, 7.0f, 0f);
            yield return new WaitForSeconds(randomSeconds);
            Instantiate(_powerUps[randomPowerUp], posToSpawn, Quaternion.identity);
        }
    }

    //private IEnumerator SpawnSpaceMineRoutine()
    //{
    //    float startRandomSeconds = Random.Range(10f, 18f);
    //    yield return new WaitForSeconds(startRandomSeconds);

    //    while (true)
    //    {
    //        float randomSeconds = Random.Range(18f, 25f);
    //        float randomX = Random.Range(-8.0f, 8.0f);
    //        Vector3 posToSpawn = new Vector3(randomX, 7.0f, 0f);
    //        yield return new WaitForSeconds(randomSeconds);
    //        Instantiate(_minePowerUp, posToSpawn, Quaternion.identity);
    //    }
    //}


    public void OnPlayerDeath()
    {
        Destroy(this.gameObject);  // Workaround for not being able to stop SpawnEnemyRoutine.
    }
}
